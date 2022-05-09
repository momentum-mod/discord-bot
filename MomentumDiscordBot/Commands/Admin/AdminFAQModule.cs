using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Commands.Moderator;

namespace MomentumDiscordBot.Commands.Admin
{
    [SlashCommandGroup("faq", "faq commands")]
    public class AdminFAQModule : ModeratorModuleBase
    {

        [SlashCommand("lock", "Locks users from being verified from the FAQ channel, use when getting spammed by user bots")]
        public async Task LockFaqAsync(InteractionContext context)
        {
            await ReplyNewEmbedAsync(context, "Since carlbot handles the FAQ roles now, in the case of a raid, please edit the `@everyone` permission override to deny viewing the channel - this will disable new user from being verified.",
                MomentumColor.Blue);
        }
    }
}
