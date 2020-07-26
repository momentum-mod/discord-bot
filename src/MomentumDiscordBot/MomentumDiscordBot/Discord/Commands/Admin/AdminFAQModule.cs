using System.Threading.Tasks;
using Discord.Commands;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord.Commands.Admin
{
    [Group("faq")]
    public class AdminFAQModule : AdminModuleBase
    {
        public FaqService FaqService { get; set; }

        [Command("reload")]
        [Summary("Reloads the FAQ service, use when the messages change")]
        public async Task ReloadFaqAsync()
        {
            await ReplyNewEmbedAsync("Reloading FAQ service...", MomentumColor.Blue);
            await FaqService.HookToLastMessageAsync();

            await ReplyNewEmbedAsync("Reloaded FAQ service", MomentumColor.Blue);
        }

        [Command("lock")]
        [Summary("Locks users from being verified from the FAQ channel, use when getting spammed by user bots")]
        public async Task LockFaqAsync()
        {
            FaqService.Lock();

            await ReplyNewEmbedAsync("Locked users from being verified in the FAQ channel", MomentumColor.Blue);
        }

        [Command("unlock")]
        [Summary("Allows users to be verified from the FAQ channel")]
        public async Task UnlockFaqAsync()
        {
            await FaqService.UnlockAsync();

            await ReplyNewEmbedAsync("Allowed users to be verified in the FAQ channel", MomentumColor.Blue);
        }

        [Command("verify")]
        [Summary("Checks if there are any unhandled reactions")]
        public async Task VerifyFaqAsync()
        {
            await FaqService.AddUnhandedReactionRolesAsync();

            await ReplyNewEmbedAsync("Done", MomentumColor.Blue);
        }
    }
}