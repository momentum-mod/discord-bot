using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Services;
using MomentumDiscordBot.Commands.Moderator;

namespace MomentumDiscordBot.Commands.Admin
{
    [Group("faq")]
    public class AdminFAQModule : ModeratorModuleBase
    {

        [Command("lock")]
        [Description("Locks users from being verified from the FAQ channel, use when getting spammed by user bots")]
        public async Task LockFaqAsync(CommandContext context)
        {
            await ReplyNewEmbedAsync(context, "Since carlbot handles the FAQ roles now, in the case of a raid, please edit the `@everyone` permission override to deny viewing the channel - this will disable new user from being verified.",
                MomentumColor.Blue);
        }
    }
}
