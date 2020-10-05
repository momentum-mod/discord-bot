using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Commands.Admin
{
    [Group("faq")]
    public class AdminFAQModule : AdminModuleBase
    {
        public FaqService FaqService { get; set; }

        [Command("reload")]
        [Description("Reloads the FAQ service, use when the messages change")]
        public async Task ReloadFaqAsync(CommandContext context)
        {
            await ReplyNewEmbedAsync(context, "Reloading FAQ service...", MomentumColor.Blue);
            await FaqService.HookToLastMessageAsync();

            await ReplyNewEmbedAsync(context, "Reloaded FAQ service", MomentumColor.Blue);
        }

        [Command("lock")]
        [Description("Locks users from being verified from the FAQ channel, use when getting spammed by user bots")]
        public async Task LockFaqAsync(CommandContext context)
        {
            FaqService.Lock();

            await ReplyNewEmbedAsync(context, "Locked users from being verified in the FAQ channel",
                MomentumColor.Blue);
        }

        [Command("unlock")]
        [Description("Allows users to be verified from the FAQ channel")]
        public async Task UnlockFaqAsync(CommandContext context)
        {
            await FaqService.UnlockAsync();

            await ReplyNewEmbedAsync(context, "Allowed users to be verified in the FAQ channel", MomentumColor.Blue);
        }

        [Command("verify")]
        [Description("Checks if there are any unhandled reactions")]
        public async Task VerifyFaqAsync(CommandContext context)
        {
            await FaqService.AddUnhandedReactionRolesAsync();

            await ReplyNewEmbedAsync(context, "Done", MomentumColor.Blue);
        }
    }
}