using System.Collections.Generic;
using Discord.WebSocket;

namespace MomentumDiscordBot.Services
{
    public class ReactionBasedRoleService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly SocketTextChannel _textChannel;
        public ReactionBasedRoleService(DiscordSocketClient discordClient, ulong channelId)
        {
            _discordClient = discordClient;
            _textChannel = _discordClient.GetChannel(channelId) as SocketTextChannel;
        }
    }
}
