using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace MomentumDiscordBot.Commands.Autocomplete
{
    public class ActivityTypeChoiceProvider : IChoiceProvider
    {
        public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            ActivityType[] types =
            {
                ActivityType.Playing,
                ActivityType.Streaming,
                ActivityType.ListeningTo,
                ActivityType.Watching,
                ActivityType.Competing
            };

            return Task.FromResult(types.Select(x => new DiscordApplicationCommandOptionChoice(x.GetName(), x.GetName())));
        }
    }
}