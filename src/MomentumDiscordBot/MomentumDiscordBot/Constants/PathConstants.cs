using System;
using System.IO;

namespace MomentumDiscordBot.Constants
{
    public static class PathConstants
    {
        internal static readonly string ConfigFolderPath = Path.Combine(Environment.CurrentDirectory, "config");
        internal static readonly string DiscordTokenFilePath = Path.Combine(ConfigFolderPath, "discordToken.txt");
        internal static readonly string TwitchAPIClientIdFilePath = Path.Combine(ConfigFolderPath, "twitchApiClientId.txt");
        internal static readonly string ConfigFilePath = Path.Combine(ConfigFolderPath, "config.json");
    }
}
