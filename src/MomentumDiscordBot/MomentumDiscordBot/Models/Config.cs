using System.IO;
using Discord;
using MomentumDiscordBot.Constants;
using Newtonsoft.Json;

namespace MomentumDiscordBot.Models
{
    public class Config
    {

        [JsonProperty("streamer_channel")] private ulong _momentumModStreamerChannelId;
        [JsonProperty("roles_channel")] private ulong _rolesChannelId;
        [JsonProperty("command_prefix")] private string _commandPrefix;
        [JsonProperty("mention_roles")] private IRole[] _mentionRoles;
        [JsonProperty("moderator_id")] private ulong _moderatorRoleID;
        [JsonProperty("admin_id")] private ulong _adminRoleID;

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
        public ulong MomentumModStreamerChannelId
        {
            get => _momentumModStreamerChannelId;
            set { _momentumModStreamerChannelId = value; SaveToFile(); }
        }
        public ulong RolesChannelId
        {
            get => _rolesChannelId;
            set { _rolesChannelId = value; SaveToFile(); }
        }
        public string CommandPrefix
        {
            get => _commandPrefix;
            set { _commandPrefix = value; SaveToFile(); }
        }
        public IRole[] MentionRoles
        {
            get => _mentionRoles;
            set { _mentionRoles = value; SaveToFile(); }
        }
        public ulong ModeratorRoleID
        {
            get => _moderatorRoleID;
            set { _moderatorRoleID = value; SaveToFile(); }
        }
        public ulong AdminRoleID
        {
            get => _adminRoleID;
            set { _adminRoleID = value; SaveToFile(); }
        }
    }
}
