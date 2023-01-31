using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using System.Collections.Concurrent;

namespace MomentumDiscordBot.Models
{
    public class CustomCommand
    {
        public CustomCommand()
        { }
        public CustomCommand(string title, string description, string user) : this(title, description, null, null, null, null, user)
        { }

        public CustomCommand(string title, string description, string buttonUrl, string buttonLabel, string thumbnailUrl, string imageUrl, string user)
        {
            this.Title = title;
            this.Description = description;
            this.ButtonUrl = buttonUrl;
            this.ButtonLabel = buttonLabel;
            this.ThumbnailUrl = thumbnailUrl;
            this.ImageUrl = imageUrl;
            this.User = user;
            this.CreationTimestamp = DateTime.Now;
        }
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("button_url")] public string ButtonUrl { get; set; }
        [JsonPropertyName("button_label")] public string ButtonLabel { get; set; }
        [JsonPropertyName("thumbnail_url")] public string ThumbnailUrl { get; set; }
        [JsonPropertyName("image_url")] public string ImageUrl { get; set; }
        [JsonPropertyName("user")] public string User { get; set; }

        [Hidden]
        [JsonPropertyName("creation_timestamp")] public DateTime CreationTimestamp { get; set; }
    }
    public class Configuration
    {
        public Configuration()
        {
            CustomCommands = new ConcurrentDictionary<string, CustomCommand>();
        }
        [JsonPropertyName("environment")] public string Environment { get; set; }
        [JsonPropertyName("bot_token")] public string BotToken { get; set; }
        [JsonPropertyName("guild_id")] public ulong GuildID { get; set; }

        [JsonPropertyName("twitch_api_client_id")]
        public string TwitchApiClientId { get; set; }

        [JsonPropertyName("twitch_api_token")] public string TwitchApiToken { get; set; }
        [JsonPropertyName("admin_id")] public ulong AdminRoleID { get; set; }

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
        
        [JsonPropertyName("stream_reannounce_backoff_minutes")]
        public int StreamReannounceBackoffMinutes { get; set; }
        
        [JsonPropertyName("stream_ping_backoff_minutes")]
        public int StreamPingBackoffMinutes { get; set; }

        [JsonPropertyName("join_log_channel")] public ulong JoinLogChannel { get; set; }

        [JsonPropertyName("message_history_channel")]
        public ulong MessageHistoryChannel { get; set; }

        [JsonPropertyName("new_account_emote")]
        public string NewUserEmoteString { get; set; }

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
        public string MySqlConnectionString { get; set; }

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

        [JsonPropertyName("custom_commands")]
        public ConcurrentDictionary<string, CustomCommand> CustomCommands { get; set; }
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
            await using var fileStream = File.Open(PathConstants.ConfigFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

            await JsonSerializer.SerializeAsync(fileStream, this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}