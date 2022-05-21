using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Commands.Checks;
using MomentumDiscordBot.Commands.Autocomplete;

namespace MomentumDiscordBot.Commands.General
{
    [RequireUserTrustedRole]
    public class GeneralModule : MomentumModuleBase
    {
        public Configuration Config { get; set; }

        [SlashCommand("say", "executes a custom command")]
        public async Task ExecCustomCommandAsync(InteractionContext context, [Autocomplete(typeof(AutoCompleteProvider))][Option("option", "name of the custom command")] string name)
        {
            CustomCommand command;
            if (Config.CustomCommands.TryGetValue(name, out command))
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = command.Title,
                    Description = command.Description,
                    Color = MomentumColor.Blue
                }.Build();
                await context.CreateResponseAsync(embed: embed);
            }
            else
            {
                await ReplyNewEmbedAsync(context, $"Command '{name}' doesn't exist", MomentumColor.Red);
            }
        }
    }
}
