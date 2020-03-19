using System;
using System.IO;

namespace MomentumDiscordBot.Constants
{
    public static class PathConstants
    {
        internal static readonly string ConfigFolderPath = Path.Combine(Environment.CurrentDirectory, "config");
        internal static readonly string DiscordTokenFilePath = Path.Combine(ConfigFolderPath, "discordToken.txt");
    }
}
