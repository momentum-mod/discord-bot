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
        }

        private async Task LoadExistingRoleEmbedsAsync()
        {
            _existingRoleEmbeds = new Dictionary<ulong, ulong>();

            var existingMessages = await _textChannel.GetMessagesAsync(limit: 200).FlattenAsync();
            
            // Filter only messages from this bot
            existingMessages = existingMessages.Where(x => x.Author.Id == _discordClient.CurrentUser.Id);

            foreach (var message in existingMessages)
            {
                if (message.Embeds.Count == 1)
                {
                    var embed = message.Embeds.First();
                    if (MentionUtils.TryParseRole(embed.Title, out var roleId))
                    {
                        _existingRoleEmbeds.Add(roleId, message.Id);
                    }
                }
            }

        }
    }
}
