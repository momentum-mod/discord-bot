using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace MomentumDiscordBot.Commands.Autocomplete
{
    public class MessageAutoCompleteProvider : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext context)
        {
            string search = context.OptionValue.ToString();
            var channelMessages = await context.Channel.GetMessagesAsync();
            return channelMessages
                .Where(x => !x.Author.IsBot && x.Author.IsSystem != true && x.Content.Contains(search))
                .Take(25)
                .Select(x => new DiscordAutoCompleteChoice(
                    string.Join("", $"{x.Author.Username}: {x.Content.Replace('\n', ' ')}".Take(100)),
                    x.Id.ToString()));
        }
    }
}