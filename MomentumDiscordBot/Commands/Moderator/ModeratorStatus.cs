using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;
using MomentumDiscordBot.Commands.Autocomplete;

namespace MomentumDiscordBot.Commands.Moderator
{
    [SlashCommandGroup("status", "bots status")]
    public class ModeratorStatus : ModeratorModuleBase
    {
        [SlashCommand("set", "Sets the bot's status")]
        public static async Task StatusAsync(InteractionContext context,
            [Option("status", "status")] string status,
            [ChoiceProvider(typeof(ActivityTypeChoiceProvider))][Option("type", "ActivityType")] string type = null)
        {
            var activity = Enum.TryParse(type, out ActivityType activityType)
                ? new DiscordActivity(status, activityType)
                : new DiscordActivity(status);
            await context.Client.UpdateStatusAsync(activity);
            await ReplyNewEmbedAsync(context, $"Status set to '{status}'.", MomentumColor.Blue);
        }

        [SlashCommand("clear", "Clears the bot's status")]
        public static async Task ClearStatusAsync(InteractionContext context)
        {
            await context.Client.UpdateStatusAsync(new DiscordActivity());
            await ReplyNewEmbedAsync(context, "Status cleared.", MomentumColor.Blue);
        }
    }
}
