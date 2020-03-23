using System.IO;
using System.Text;
using Discord;
using MomentumDiscordBot.Constants;
using Newtonsoft.Json;

namespace MomentumDiscordBot.Models
{
    public class Config
    {
        [JsonProperty("admin_id")] private ulong _adminRoleID;
        [JsonProperty("command_prefix")] private string _commandPrefix;
        [JsonProperty("livestream_mention_role_id")] private ulong _livestreamMentionRoleId;
        [JsonProperty("mention_role_emoji")] private string _mentionRoleEmojiString;
        [JsonProperty("mention_roles")] private ulong[] _mentionRoles;
        [JsonProperty("moderator_id")] private ulong _moderatorRoleID;
        [JsonProperty("streamer_channel")] private ulong _momentumModStreamerChannelId;
        [JsonProperty("roles_channel")] private ulong _rolesChannelId;
        [JsonProperty("twitch_user_bans")] private string[] _twitchUserBans;

        [JsonIgnore]
        public ulong MomentumModStreamerChannelId
        {
            get => _momentumModStreamerChannelId;
            set
            {
                _momentumModStreamerChannelId = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public ulong RolesChannelId
        {
            get => _rolesChannelId;
            set
            {
                _rolesChannelId = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public string CommandPrefix
        {
            get => _commandPrefix;
            set
            {
                _commandPrefix = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public ulong[] MentionRoles
        {
            get => _mentionRoles;
            set
            {
                _mentionRoles = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public ulong ModeratorRoleID
        {
            get => _moderatorRoleID;
            set
            {
                _moderatorRoleID = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public ulong AdminRoleID
        {
            get => _adminRoleID;
            set
            {
                _adminRoleID = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public string MentionRoleEmojiString
        {
            get => _mentionRoleEmojiString;
            set
            {
                _mentionRoleEmojiString = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public ulong LivestreamMentionRoleId
        {
            get => _livestreamMentionRoleId;
            set
            {
                _livestreamMentionRoleId = value;
                SaveToFile();
            }
        }

        [JsonIgnore] public Emoji MentionRoleEmoji => new Emoji(MentionRoleEmojiString);

        [JsonIgnore]
        public string[] TwitchUserBans
        {
            get => _twitchUserBans;
            set
            {
                _twitchUserBans = value;
                SaveToFile();
            }
        }

        public static Config LoadFromFile()
        {
            if (File.Exists(PathConstants.ConfigFilePath))
            {
                // File exists, get the text
                var configString = File.ReadAllText(PathConstants.ConfigFilePath, Encoding.UTF8);
                return JsonConvert.DeserializeObject<Config>(configString);
            }

            throw new FileNotFoundException($"No config file exists, expected it at: '{PathConstants.ConfigFilePath}'");
        }

        private void SaveToFile()
        {
            var configString = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(PathConstants.ConfigFilePath, configString, Encoding.UTF8);
        }
    }
}