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

        public static bool IsSelf(this IUser user, DiscordSocketClient discordClient)
            => discordClient.CurrentUser.Id == user.Id;
        public static string EscapeDiscordChars(this string source)
        {
            var chars = new List<string> { "*", "_", "~", "`", "@", ">", "||" };
            return chars.Aggregate(source, (current, character) => current.Replace($"{character}", $@"\{character}"));
        }
    }
}
