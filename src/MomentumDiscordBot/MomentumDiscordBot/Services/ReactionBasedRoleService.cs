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
        private readonly SocketTextChannel _textChannel;
        // <RoleID, MessageID>
        private Dictionary<ulong, ulong> _existingRoleEmbeds;
        public ReactionBasedRoleService(DiscordSocketClient discordClient, Config config)
        {
            _discordClient = discordClient;
            _textChannel = _discordClient.GetChannel(config.RolesChannelId) as SocketTextChannel;

            LoadExistingRoleEmbedsAsync().GetAwaiter().GetResult();

            _discordClient.ReactionAdded += ReactionAdded;
            _discordClient.ReactionRemoved += ReactionRemoved;
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
                if (MentionUtils.TryParseRole(embed.Title, out var roleId))
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
            if (_existingRoleEmbeds.ContainsKey(reaction.MessageId))
            {
                var user = _discordClient.GetUser(reaction.UserId);
                if (user is SocketGuildUser guildUser && TryParseRoleFromEmbed(messageBefore.Value, out var role))
                { 
                    await guildUser.AddRoleAsync(role);
                }
            }
        }

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> messageBefore, ISocketMessageChannel messageAfter, SocketReaction reaction)
        {
            if (reaction.Channel.Id != _textChannel.Id) return;

            // Check that the message reacted to is a role embed
            if (_existingRoleEmbeds.ContainsKey(reaction.MessageId))
            {
                var user = _discordClient.GetUser(reaction.UserId);
                if (user is SocketGuildUser guildUser && TryParseRoleFromEmbed(messageBefore.Value, out var role))
                {
                    await guildUser.RemoveRoleAsync(role);
                }
            }
        }
    }
}
