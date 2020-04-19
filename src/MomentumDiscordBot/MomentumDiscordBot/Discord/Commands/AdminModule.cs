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

        [Command("lockfaq")]
        [Summary("Locks users from being verified from the FAQ channel, use when getting spammed by user bots")]
        public async Task LockFaqAsync()
        {
            FaqService.IsEnabled = false;

            await ReplyNewEmbedAsync("Locked users from being verified in the FAQ channel", Color.Blue);
        }

        [Command("unlockfaq")]
        [Summary("Allows users to be verified from the FAQ channel")]
        public async Task UnlockFaqAsync()
        {
            FaqService.IsEnabled = true;

            await ReplyNewEmbedAsync("Allows users to be verified in the FAQ channel", Color.Blue);
        }
    }
}