using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Services
{
    public class ReactionBasedRoleService
    {
        private readonly DiscordSocketClient _discordClient;
        private SocketTextChannel _textChannel;

        private readonly Config _config;
        // <RoleID, MessageID>
        private Dictionary<ulong, ulong> _existingRoleEmbeds;
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

        private async Task LoadExistingRoleEmbedsAsync()
        {
            _existingRoleEmbeds = new Dictionary<ulong, ulong>();

            var existingMessages = await _textChannel.GetMessagesAsync(limit: 200).FlattenAsync();
            
            // Filter only messages from this bot
            existingMessages = existingMessages.Where(x => x.Author.Id == _discordClient.CurrentUser.Id);

            foreach (var message in existingMessages)
            {
                if (TryParseRoleFromEmbed(message, out var role))
                {
                    _existingRoleEmbeds.Add(role.Id, message.Id);
                }
            }
        }

        /// <summary>
        /// The role should be @Mentioned in the Title of the Embed
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

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> messageBefore, ISocketMessageChannel messageAfter, SocketReaction reaction)
        {
            if (reaction.Channel.Id != _textChannel.Id) return;

            // Check that the message reacted to is a role embed
            if (_existingRoleEmbeds.ContainsValue(reaction.MessageId))
            {
                // Get the user as a SocketGuildContext
                var user = _discordClient.Guilds.First(x => x.Channels.Select(x => x.Id).Contains(messageAfter.Id))
                    .Users.First(x => x.Id == reaction.UserId);

                var message = await messageBefore.GetOrDownloadAsync();
                if (TryParseRoleFromEmbed(message, out var role))
                { 
                    await user.AddRoleAsync(role);
                }
            }
        }

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> messageBefore, ISocketMessageChannel messageAfter, SocketReaction reaction)
        {
            if (reaction.Channel.Id != _textChannel.Id) return;

            // Check that the message reacted to is a role embed
            if (_existingRoleEmbeds.ContainsValue(reaction.MessageId))
            {
                // Get the user as a SocketGuildContext
                var user = _discordClient.Guilds.First(x => x.Channels.Select(x => x.Id).Contains(messageAfter.Id))
                    .Users.First(x => x.Id == reaction.UserId);
                var message = await messageBefore.GetOrDownloadAsync();
                if (TryParseRoleFromEmbed(message, out var role))
                {
                    await user.RemoveRoleAsync(role);
                }
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
