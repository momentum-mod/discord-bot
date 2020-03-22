using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace MomentumDiscordBot.Utilities
{
    public static class DiscordExtensions
    {
        public static IEnumerable<IMessage> FromSelf(this IEnumerable<IMessage> source,
            DiscordSocketClient discordClient)
            => source.Where(x => x.Author.Id == discordClient.CurrentUser.Id);
    }
}
