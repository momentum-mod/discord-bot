using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Commands.Moderator
{
    [SlashCommandGroup("statstop", "shows stats of top ...")]
    public class StatsTopModule : ModeratorModuleBase
    {
        public Configuration Config { get; set; }

        [SlashCommand("users", "shows stats of top users")]
        public async Task TopUsersAsync(InteractionContext context)
        {
            var topUsers = await StatsUtility.GetTopMessages(Config, x => x.UserId);

            var embedBuilder = topUsers.GetTopStatsEmbedBuilder("Most Active Users",
                x =>
                    $"{context.Guild.Members.Values.FirstOrDefault(y => y.Id == x.Grouping)?.Mention ?? x.Grouping.ToString()} - {x.MessageCount} messages");


            await context.CreateResponseAsync(embed: embedBuilder.Build());
        }

        [SlashCommand("channels", "shows stats of top channels")]
        public async Task TopChannelsAsync(InteractionContext context)
        {
            var topChannels = await StatsUtility.GetTopMessages(Config, x => x.ChannelId);

            var embedBuilder = topChannels.GetTopStatsEmbedBuilder("Most Active Channels",
                x =>
                    $"{context.Guild.Channels.Values.FirstOrDefault(y => y.Id == x.Grouping)?.Mention ?? x.Grouping.ToString()} - {x.MessageCount} messages");

            await context.CreateResponseAsync(embed: embedBuilder.Build());
        }
    }
}
