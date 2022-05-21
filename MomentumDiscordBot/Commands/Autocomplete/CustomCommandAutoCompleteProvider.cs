using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Entities;
using MomentumDiscordBot.Models;
using DSharpPlus.SlashCommands;

namespace MomentumDiscordBot.Commands.Autocomplete
{
    public class AutoCompleteProvider : IAutocompleteProvider
    {
        private IEnumerable<string> findCommand(IEnumerable<string> commands, string s)
        {
            return commands.Where(x => x.Contains(s)).Take(25).OrderBy(x => x);
        }
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext context)
        {
            var commands = context.Services.GetRequiredService<Configuration>().CustomCommands.Keys;
            var choices = findCommand(commands, context.OptionValue.ToString());
            if (!choices.Any())
            {
                DiscordEmoji emoji;
                if (DiscordEmoji.TryFromName(context.Client, context.OptionValue.ToString(), out emoji))
                {
                    choices = findCommand(commands, emoji.ToString());
                }
            }
            return Task.FromResult(choices.Select(command => new DiscordAutoCompleteChoice(command, command)));
        }
    }
}