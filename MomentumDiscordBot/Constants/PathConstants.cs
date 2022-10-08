using System;
using System.IO;

namespace MomentumDiscordBot.Constants
{
    internal static class PathConstants
    {
        private static readonly string ConfigFolderPath = Path.Combine(Environment.CurrentDirectory, "config");
        internal static readonly string ConfigFilePath = Path.Combine(ConfigFolderPath, "config.json");
        
        private static readonly string DbFolderPath = Path.Combine(Environment.CurrentDirectory, "data");
        internal static readonly string DbFilePath = Path.Combine(DbFolderPath, "bot_data.db");
    }
}