using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Commands.Autocomplete
{
    public class AutoCompleteProvider : IAutocompleteProvider
    {
        private static IEnumerable<string> FindCommand(IEnumerable<string> commands, string s)
        {
            return commands.Where(x => x.Contains(s)).Take(25).OrderBy(x => x);
        }
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext context)
        {
            var commands = context.Services.GetRequiredService<Configuration>().CustomCommands.Keys;
            var choices = FindCommand(commands, context.OptionValue.ToString());
            if (!choices.Any())
            {
                if (DiscordEmoji.TryFromName(context.Client, context.OptionValue.ToString(), out DiscordEmoji emoji))
                {
                    choices = FindCommand(commands, emoji.ToString());
                }
            }
            return Task.FromResult(choices.Select(command => new DiscordAutoCompleteChoice(command, command)));
        }
    }

    public class CustomCommandPropertyChoiceProvider : IChoiceProvider
    {
        public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            var properties = typeof(CustomCommand).GetProperties();
            var choices = properties.Where(x => !x.GetCustomAttributes()
                                    .Any(x => x.GetType() == typeof(HiddenAttribute)))
                                    .Take(25)
                                    .Select(property => new DiscordApplicationCommandOptionChoice(property.Name, property.Name));
            return Task.FromResult(choices);
        }
    }
}