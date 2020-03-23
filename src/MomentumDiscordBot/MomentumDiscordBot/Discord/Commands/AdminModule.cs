using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Discord.Precondition;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord.Commands
{
    [AdminPrecondition]
    [RequireAdminBotChannelPrecondition]
    public class AdminModule : MomentumModuleBase
    {
        public StreamMonitorService StreamMonitorService { get; set; }

        [Command("updatestreams")]
        public async Task ForceUpdateStreamsAsync()
        {
            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync("Updating Livestreams", Color.Blue);
        }
    }
}