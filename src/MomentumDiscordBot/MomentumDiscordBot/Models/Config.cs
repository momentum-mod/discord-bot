using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MomentumDiscordBot.Models
{
    public class Config
    {
        [JsonProperty("streamer_channel")] public ulong MomentumModStreamerChannelId { get; set; }
    }
}
