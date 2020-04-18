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
        public FaqService FaqService { get; set; }

        [Command("updatestreams")]
        [Summary("Force an update of Twitch livestreams")]
        public async Task ForceUpdateStreamsAsync()
        {
            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync("Updating Livestreams", Color.Blue);
        }

        [Command("reloadfaq")]
        [Summary("Reloads the FAQ service, use when the messages change")]
        public async Task ReloadFaqAsync()
        {
            await FaqService.HookToLastMessageAsync();

            await ReplyNewEmbedAsync("Reloaded FAQ service", Color.Blue);
        }
    }
}