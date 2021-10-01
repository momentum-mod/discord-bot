using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;

namespace MomentumDiscordBot.Models
{
    public class Configuration
    {
        [JsonPropertyName("environment")] public string Environment { get; set; }
        [JsonPropertyName("bot_token")] public string BotToken { get; set; }

        [JsonPropertyName("twitch_api_client_id")]
        public string TwitchApiClientId { get; set; }

        [JsonPropertyName("twitch_api_token")] public string TwitchApiToken { get; set; }
        [JsonPropertyName("admin_id")] public ulong AdminRoleID { get; set; }
        [JsonPropertyName("command_prefix")] public string CommandPrefix { get; set; }

        [JsonPropertyName("livestream_mention_role_id")]
        public ulong LivestreamMentionRoleId { get; set; }

        [JsonPropertyName("mention_role_emoji")]
        public string MentionRoleEmojiString { get; set; }

        [JsonPropertyName("faq_role_emoji")] public string FaqRoleEmojiString { get; set; }
        [JsonPropertyName("mention_roles")] public ulong[] MentionRoles { get; set; }
        [JsonPropertyName("moderator_id")] public ulong ModeratorRoleID { get; set; }
        [JsonPropertyName("streamer_channel")] public ulong MomentumModStreamerChannelId { get; set; }
        [JsonPropertyName("roles_channel")] public ulong RolesChannelId { get; set; }
        [JsonPropertyName("twitch_user_bans")] public string[] TwitchUserBans { get; set; }

        [JsonPropertyName("admin_bot_channel")]
        public ulong AdminBotChannel { get; set; }

        [JsonPropertyName("stream_update_interval")]
        public int StreamUpdateInterval { get; set; }

        [JsonPropertyName("key_emoji")] public string KeyEmojiString { get; set; }

        [JsonPropertyName("key_begging_response")]
        public string KeyBeggingResponse { get; set; }

        [JsonPropertyName("key_regex")] public string KeyRegexString { get; set; }
        [JsonPropertyName("join_log_channel")] public ulong JoinLogChannel { get; set; }

        [JsonPropertyName("message_history_channel")]
        public ulong MessageHistoryChannel { get; set; }

        [JsonPropertyName("new_account_emote")]
        public string NewUserEmoteString { get; set; }

        [JsonPropertyName("whitelist_key_begging_roles")]
        public ulong[] WhitelistKeyBeggingRoles { get; set; }

        [JsonPropertyName("minimum_stream_viewers_announce")]
        public int MinimumStreamViewersAnnounce { get; set; }

        [JsonPropertyName("seq_address")] public string SeqAddress { get; set; }
        [JsonPropertyName("seq_token")] public string SeqToken { get; set; }
        [JsonPropertyName("faq_channel")] public ulong FaqChannelId { get; set; }
        [JsonPropertyName("faq_role")] public ulong FaqRoleId { get; set; }
        [JsonPropertyName("developer_id")] public ulong DeveloperID { get; set; }

        [JsonPropertyName("alt_account_emoji")]
        public string AltAccountEmojiString { get; set; }

        [Hidden]
        [JsonPropertyName("mysql_connection_string")]
        public Microsoft.EntityFrameworkCore.ServerVersion MySqlConnectionString { get; set; }

        [JsonPropertyName("media_verified_role")]
        public ulong MediaVerifiedRoleId { get; set; }

        [JsonPropertyName("media_blacklisted_role")]
        public ulong MediaBlacklistedRoleId { get; set; }

        [JsonPropertyName("media_minimum_days")]
        public int MediaMinimumDays { get; set; }

        [JsonPropertyName("media_minimum_messages")]
        public int MediaMinimumMessages { get; set; }

        [JsonIgnore] public DiscordEmoji MentionRoleEmoji => DiscordEmoji.FromUnicode(MentionRoleEmojiString);
        [JsonIgnore] public DiscordEmoji FaqRoleEmoji => DiscordEmoji.FromUnicode(FaqRoleEmojiString);
        [JsonIgnore] public DiscordEmoji AltAccountEmoji => DiscordEmoji.FromUnicode(AltAccountEmojiString);

        public static async Task<Configuration> LoadFromFileAsync()
        {
            if (!File.Exists(PathConstants.ConfigFilePath))
            {
                throw new FileNotFoundException(
                    $"No config file exists, expected it at: '{PathConstants.ConfigFilePath}'");
            }

            // File exists, get the config
            await using var fileStream = File.OpenRead(PathConstants.ConfigFilePath);
            return await JsonSerializer.DeserializeAsync<Configuration>(fileStream);
        }

        public async Task SaveToFileAsync()
        {
            await using var fileStream = File.Open(PathConstants.ConfigFilePath, FileMode.Create);

            await JsonSerializer.SerializeAsync(fileStream, this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}