using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Services
{
    public class ReactionBasedRoleService
    {
        private readonly Config _config;

        private readonly DiscordSocketClient _discordClient;

        // <RoleID, MessageID>
        private Dictionary<ulong, ulong> _existingRoleEmbeds;
        private SocketTextChannel _textChannel;

        public ReactionBasedRoleService(DiscordSocketClient discordClient, Config config)
        {
            _config = config;

            _discordClient = discordClient;
            _discordClient.Ready += _discordClient_Ready;
            _discordClient.ReactionAdded += ReactionAdded;
            _discordClient.ReactionRemoved += ReactionRemoved;
        }

        private async Task _discordClient_Ready()
        {
            _textChannel = _discordClient.GetChannel(_config.RolesChannelId) as SocketTextChannel;

            await LoadExistingRoleEmbedsAsync();
            await SendRoleEmbedsAsync();
            await VerifyCurrentUserRolesAsync();
        }

        private async Task LoadExistingRoleEmbedsAsync()
        {
            _existingRoleEmbeds = new Dictionary<ulong, ulong>();

            var existingMessages = await _textChannel.GetMessagesAsync(200).FlattenAsync();

            // Filter only messages from this bot
            existingMessages = existingMessages.FromSelf(_discordClient);

            foreach (var message in existingMessages)
            {
                if (TryParseRoleFromEmbed(message, out var role))
                {
                    _existingRoleEmbeds.Add(role.Id, message.Id);
                }
            }
        }

        private async Task SendRoleEmbedsAsync()
        {
            // If there are roles added to the config, but aren't sent yet, send them
            if (_config.MentionRoles != null)
            {
                foreach (var mentionRole in _config.MentionRoles)
                {
                    if (!_existingRoleEmbeds.ContainsKey(mentionRole))
                    {
                        var role = _textChannel.Guild.Roles.First(x => x.Id == mentionRole);
                        await SendRoleEmbed(role);
                    }
                }
            }
        }

        private async Task VerifyCurrentUserRolesAsync()
        {
            var usersWithMentionRoles = _textChannel.Guild.Users.Where(x => _config.MentionRoles.Intersect(x.Roles.Select(y => y.Id)).Any()).ToList();

            // Check users who have reacted to the embed
            foreach (var (roleId, messageId) in _existingRoleEmbeds)
            {
                var message = await _textChannel.GetMessageAsync(messageId);
                var role = _textChannel.Guild.GetRole(roleId);

                if (!(message is IUserMessage userMessage)) continue;

                // Get all users who have reacted to the embed
                var reactionUsers = (await userMessage.GetReactionUsersAsync(_config.MentionRoleEmoji, _textChannel.Guild.MemberCount).FlattenAsync()).ToList();

                foreach (var guildUser in reactionUsers.Where(user => !user.IsSelf(_discordClient))
                    .Where(user =>
                        !usersWithMentionRoles.Any(x => x.Roles.Any(y => y.Id == roleId) && x.Id == user.Id))
                    .Select(user => _textChannel.Guild.GetUser(user.Id)))
                {
                    // User without role
                    await guildUser.AddRoleAsync(role);
                }

                var userWithRole = usersWithMentionRoles.Where(x => x.Roles.Any(x => x.Id == roleId));
                foreach (var user in userWithRole)
                {
                    if (reactionUsers.Any(x => x.Id == user.Id) && !user.IsSelf(_discordClient)) continue;
                        
                    // User has not reacted, remove the role
                    var guildUser = _textChannel.Guild.GetUser(user.Id);
                    await guildUser.RemoveRoleAsync(role);
                }
            }

        }

        /// <summary>
        ///     The role should be @Mentioned in the Description of the Embed
        /// </summary>
        private bool TryParseRoleFromEmbed(IMessage input, out IRole role)
        {
            if (input.Embeds.Count == 1)
            {
                var embed = input.Embeds.First();
                if (MentionUtils.TryParseRole(embed.Description, out var roleId))
                {
                    // We have the role ID, want the IRole - search guilds for the correct channel
                    var guild = _discordClient.Guilds.First(x => x.Channels.Any(y => y.Id == _textChannel.Id));
                    role = guild.GetRole(roleId);
                    return true;
                }
            }

            role = null;
            return false;
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> messageBefore,
            ISocketMessageChannel messageAfter, SocketReaction reaction)
        {
            if (reaction.Channel.Id != _textChannel.Id || !reaction.Emote.Equals(_config.MentionRoleEmoji)) return;

            // Check that the message reacted to is a role embed
            if (_existingRoleEmbeds.ContainsValue(reaction.MessageId))
            {
                // Get the user as a SocketGuildContext
                var user = _discordClient.Guilds.First(x => x.Channels.Select(x => x.Id).Contains(messageAfter.Id))
                    .Users.First(x => x.Id == reaction.UserId);

                // Ignore actions from the bot
                if (user.IsSelf(_discordClient)) return;

                var message = await messageBefore.GetOrDownloadAsync();
                if (TryParseRoleFromEmbed(message, out var role)) await user.AddRoleAsync(role);
            }
        }

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> messageBefore,
            ISocketMessageChannel messageAfter, SocketReaction reaction)
        {
            if (reaction.Channel.Id != _textChannel.Id || !reaction.Emote.Equals(_config.MentionRoleEmoji)) return;

            // Check that the message reacted to is a role embed
            if (_existingRoleEmbeds.ContainsValue(reaction.MessageId))
            {
                // Get the user as a SocketGuildContext
                var user = _discordClient.Guilds.First(x => x.Channels.Select(x => x.Id).Contains(messageAfter.Id))
                    .Users.First(x => x.Id == reaction.UserId);

                // Ignore actions from the bot
                if (user.IsSelf(_discordClient)) return;

                var message = await messageBefore.GetOrDownloadAsync();
                if (TryParseRoleFromEmbed(message, out var role)) await user.RemoveRoleAsync(role);
            }
        }

        public async Task SendRoleEmbed(IRole role)
        {
            // If the role isn't already sent
            if (!_existingRoleEmbeds.TryGetValue(role.Id, out _))
            {
                var embed = new EmbedBuilder
                {
                    Description = MentionUtils.MentionRole(role.Id),
                    Color = role.Color
                }.Build();

                var message = await _textChannel.SendMessageAsync(embed: embed);
                await message.AddReactionAsync(_config.MentionRoleEmoji);

                _existingRoleEmbeds.Add(role.Id, message.Id);
            }
        }

        public async Task RemoveRoleEmbed(IRole role)
        {
            // If the role is sent
            if (_existingRoleEmbeds.TryGetValue(role.Id, out var messageId))
            {
                var message = await _textChannel.GetMessageAsync(messageId);
                await message.DeleteAsync();

                _existingRoleEmbeds.Remove(role.Id);
            }
        }
    }
}