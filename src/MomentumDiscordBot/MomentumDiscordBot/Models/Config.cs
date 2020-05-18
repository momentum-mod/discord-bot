using System.IO;
using System.Linq;
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
        [JsonProperty("admin_bot_channel")] private ulong _adminBotChannel;
        [JsonProperty("stream_update_interval")] private int _streamUpdateInterval;
        [JsonProperty("key_emoji")] private string _keyEmojiString;
        [JsonProperty("key_begging_response")] private string _keyBeggingResponse;
        [JsonProperty("join_log_channel")] private ulong _joinLogChannel;
        [JsonProperty("message_history_channel")] private ulong _messageHistoryChannel;

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
                _mentionRoles = value.Distinct().ToArray();
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
                _twitchUserBans = value.Distinct().ToArray();
                SaveToFile();
            }
        }
        [JsonIgnore] 
        public ulong AdminBotChannel
        {
            get => _adminBotChannel;
            set
            {
                _adminBotChannel = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public int StreamUpdateInterval
        {
            get => _streamUpdateInterval;
            set
            {
                _streamUpdateInterval = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public string KeyEmojiString
        {
            get => _keyEmojiString;
            set
            {
                _keyEmojiString = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public ulong JoinLogChannel
        {
            get => _joinLogChannel;
            set
            {
                _joinLogChannel = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public string KeyBeggingResponse
        {
            get => _keyBeggingResponse;
            set
            {
                _keyBeggingResponse = value;
                SaveToFile();
            }
        }

        [JsonIgnore]
        public ulong MessageHistoryChannel
        {
            get => _messageHistoryChannel;
            set
            {
                _messageHistoryChannel = value;
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