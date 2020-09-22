using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Discord.Commands.Moderator
{
    [Group("stats top")]
    public class StatsTopModule : ModeratorModuleBase
    {
        public Config Config { get; set; }

        [Command("users")]
        public async Task TopUsersAsync()
        {
            var topUsers = await StatsUtility.GetTopMessages(Config, x => x.UserId);

            var embedBuilder = topUsers.GetTopStatsEmbedBuilder("Most Active Users",
                x => $"{MentionUtils.MentionUser(x.Grouping)} - {x.MessageCount} messages");


            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("channels")]
        public async Task TopChannelsAsync()
        {
            var topChannels = await StatsUtility.GetTopMessages(Config, x => x.ChannelId);

            var embedBuilder = topChannels.GetTopStatsEmbedBuilder("Most Active Channels",
                x => $"{MentionUtils.MentionChannel(x.Grouping)} - {x.MessageCount} messages");

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}