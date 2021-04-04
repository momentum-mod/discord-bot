using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Commands.Moderator
{
    [Group("statstop")]
    public class StatsTopModule : ModeratorModuleBase
    {
        public Configuration Config { get; set; }

        [Command("users")]
        public async Task TopUsersAsync(CommandContext context)
        {
            var topUsers = await StatsUtility.GetTopMessages(Config, x => x.UserId);

            var embedBuilder = topUsers.GetTopStatsEmbedBuilder("Most Active Users",
                x =>
                    $"{context.Guild.Members.Values.FirstOrDefault(y => y.Id == x.Grouping)?.Mention ?? x.Grouping.ToString()} - {x.MessageCount} messages");


            await context.RespondAsync(embed: embedBuilder.Build());
        }

        [Command("channels")]
        public async Task TopChannelsAsync(CommandContext context)
        {
            var topChannels = await StatsUtility.GetTopMessages(Config, x => x.ChannelId);

            var embedBuilder = topChannels.GetTopStatsEmbedBuilder("Most Active Channels",
                x =>
                    $"{context.Guild.Channels.Values.FirstOrDefault(y => y.Id == x.Grouping)?.Mention ?? x.Grouping.ToString()} - {x.MessageCount} messages");

            await context.RespondAsync(embed: embedBuilder.Build());
        }
    }
}