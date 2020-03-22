using System;
using System.IO;
using MomentumDiscordBot.Constants;
using Newtonsoft.Json;

namespace MomentumDiscordBot.Models
{
    public class Config
    {
        private ulong _momentumModStreamerChannelId;
        private ulong _rolesChannelId;
        private string _commandPrefix;

        [JsonConstructor]
        public Config() { }

        public static Config LoadFromFile()
        {
            if (File.Exists(PathConstants.ConfigFilePath))
            {
                // File exists, get the text
                var configString = File.ReadAllText(PathConstants.ConfigFilePath);
                return JsonConvert.DeserializeObject<Config>(configString);
            }
            else
            {
                throw new FileNotFoundException($"No config file exists, expected it at: '{PathConstants.ConfigFilePath}'");
            }
        }

        private void SaveToFile()
        {
            var configString = JsonConvert.SerializeObject(this);
            File.WriteAllText(PathConstants.ConfigFilePath, configString);
        }

        [JsonProperty("streamer_channel")]
        public ulong MomentumModStreamerChannelId
        {
            get => _momentumModStreamerChannelId;
            set { _momentumModStreamerChannelId = value; SaveToFile(); }
        }

        [JsonProperty("roles_channel")]
        public ulong RolesChannelId
        {
            get => _rolesChannelId;
            set { _rolesChannelId = value; SaveToFile(); }
        }

        [JsonProperty("command_prefix")]
        public string CommandPrefix
        {
            get => _commandPrefix;
            set { _commandPrefix = value; SaveToFile(); }
        }
    }
}
