using Newtonsoft.Json;

namespace MomentumDiscordBot.Models
{
    public class Config
    {
        [JsonProperty("streamer_channel")] public ulong MomentumModStreamerChannelId { get; set; }
        [JsonProperty("roles_channel")] public ulong RolesChannelId { get; set; }
        [JsonProperty("command_prefix")] public string CommandPrefix { get; set; }
    }
}
