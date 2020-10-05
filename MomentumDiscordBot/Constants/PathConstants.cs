using System;
using System.IO;

namespace MomentumDiscordBot.Constants
{
    internal static class PathConstants
    {
        private static readonly string ConfigFolderPath = Path.Combine(Environment.CurrentDirectory, "config");
        internal static readonly string ConfigFilePath = Path.Combine(ConfigFolderPath, "config.json");
    }
}