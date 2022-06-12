using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace MomentumDiscordBot.Commands.Autocomplete
{
    public class TimezoneAutoCompleteProvider : IAutocompleteProvider
    {
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext context)
        {
            string search = context.OptionValue.ToString().ToLower();
            IEnumerable<TimeZoneInfo> choices = TimeZoneInfo.GetSystemTimeZones();
            if (!string.IsNullOrWhiteSpace(search))
                choices = choices.Where(x => x.Id.ToLower().Contains(search));
            return Task.FromResult(choices.Take(25).Select(x => new DiscordAutoCompleteChoice(x.Id, x.Id)));
        }
    }
}