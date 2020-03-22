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
        [JsonProperty("mention_roles")] private ulong[] _mentionRoles;
        [JsonProperty("moderator_id")] private ulong _moderatorRoleID;
        [JsonProperty("admin_id")] private ulong _adminRoleID;
        [JsonProperty("mention_role_emoji")] private string _mentionRoleEmojiString;
        [JsonProperty("livestream_mention_role_id")] private ulong _livestreamMentionRoleId;

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
            var configString = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(PathConstants.ConfigFilePath, configString);
        }
        
        [JsonIgnore] public ulong MomentumModStreamerChannelId
        {
            get => _momentumModStreamerChannelId;
            set { _momentumModStreamerChannelId = value; SaveToFile(); }
        }
        [JsonIgnore] public ulong RolesChannelId
        {
            get => _rolesChannelId;
            set { _rolesChannelId = value; SaveToFile(); }
        }
        [JsonIgnore] public string CommandPrefix
        {
            get => _commandPrefix;
            set { _commandPrefix = value; SaveToFile(); }
        }
        [JsonIgnore] public ulong[] MentionRoles
        {
            get => _mentionRoles;
            set { _mentionRoles = value; SaveToFile(); }
        }
        [JsonIgnore] public ulong ModeratorRoleID
        {
            get => _moderatorRoleID;
            set { _moderatorRoleID = value; SaveToFile(); }
        }
        [JsonIgnore] public ulong AdminRoleID
        {
            get => _adminRoleID;
            set { _adminRoleID = value; SaveToFile(); }
        }

        [JsonIgnore] public string MentionRoleEmojiString
        {
            get => _mentionRoleEmojiString;
            set { _mentionRoleEmojiString = value; SaveToFile(); }
        }
        [JsonIgnore] public ulong LivestreamMentionRoleId
        {
            get => _livestreamMentionRoleId;
            set { _livestreamMentionRoleId = value; SaveToFile(); }
        }
        [JsonIgnore] public Emoji MentionRoleEmoji => new Emoji(MentionRoleEmojiString);
    }
}
