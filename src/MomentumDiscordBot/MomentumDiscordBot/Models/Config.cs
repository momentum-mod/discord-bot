using System;
using System.IO;
using System.Text;
using Discord;
using MomentumDiscordBot.Constants;
using Newtonsoft.Json;

namespace MomentumDiscordBot.Models
{
    public class Config
    {
        [JsonProperty("environment")] public string Environment { get; set; }
        [JsonProperty("admin_id")] public ulong AdminRoleID { get; set; }
        [JsonProperty("command_prefix")] public string CommandPrefix { get; set; }
        [JsonProperty("livestream_mention_role_id")] public ulong LivestreamMentionRoleId { get; set; }
        [JsonProperty("mention_role_emoji")] public string MentionRoleEmojiString { get; set; }
        [JsonProperty("mention_roles")] public ulong[] MentionRoles { get; set; }
        [JsonProperty("moderator_id")] public ulong ModeratorRoleID { get; set; }
        [JsonProperty("streamer_channel")] public ulong MomentumModStreamerChannelId { get; set; }
        [JsonProperty("roles_channel")] public ulong RolesChannelId { get; set; }
        [JsonProperty("twitch_user_bans")] public string[] TwitchUserBans { get; set; }
        [JsonProperty("admin_bot_channel")] public ulong AdminBotChannel { get; set; }
        [JsonProperty("stream_update_interval")] public int StreamUpdateInterval { get; set; }
        [JsonProperty("key_emoji")] public string KeyEmojiString { get; set; }
        [JsonProperty("key_begging_response")] public string KeyBeggingResponse { get; set; }
        [JsonProperty("key_regex")] public string KeyRegexString { get; set; }
        [JsonProperty("join_log_channel")] public ulong JoinLogChannel { get; set; }
        [JsonProperty("message_history_channel")] public ulong MessageHistoryChannel { get; set; }
        [JsonProperty("new_account_emote")] public string NewUserEmoteString { get; set; }
        [JsonProperty("whitelist_key_begging_roles")] public ulong[] WhitelistKeyBeggingRoles { get; set; }
        [JsonProperty("minimum_stream_viewers_announce")] public int MinimumStreamViewersAnnounce { get; set; }
        [JsonProperty("seq_address")] public string SeqAddress { get; set; }
        [JsonProperty("seq_token")] public string SeqToken { get; set; }
        [JsonProperty("developer_id")] public ulong DeveloperID { get; set; }

        [JsonIgnore] public Emoji MentionRoleEmoji => new Emoji(MentionRoleEmojiString);
        public static Config LoadFromFile()
        {
            if (File.Exists(PathConstants.ConfigFilePath))
            {
                // File exists, get the text
                var configString = File.ReadAllText(PathConstants.ConfigFilePath, Encoding.UTF8);
                return JsonConvert.DeserializeObject<Config>(configString, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    Error = LoadConfigErrorHandler
                });
            }

            throw new FileNotFoundException($"No config file exists, expected it at: '{PathConstants.ConfigFilePath}'");
        }

        private static void LoadConfigErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            Console.WriteLine(e.ErrorContext.Error.Message);
            e.ErrorContext.Handled = true;
        }

        public void SaveToFile()
        {
            var configString = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(PathConstants.ConfigFilePath, configString, Encoding.UTF8);
        }
    }
}