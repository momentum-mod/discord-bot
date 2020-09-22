using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Discord.Commands.Moderator
{
    [Group("stats")]
    public class StatsModule : ModeratorModuleBase
    {
        public Config Config { get; set; }

        [Command("")]
        public async Task UserStatsAsync(IUser user)
        {
            var totalMessageCount = await StatsUtility.GetTotalMessageCount(Config);
            var userStats = await StatsUtility.GetMessages(Config, x => x.UserId == user.Id);

            var embedBuilder = new EmbedBuilder
                {
                    Title = "User Stats",
                    Color = MomentumColor.Blue
                }.WithAuthor(user)
                .AddField("Total Messages",
                    $"{userStats.Sum(x => x.MessageCount)} - {(decimal) userStats.Sum(x => x.MessageCount) / totalMessageCount:P} of total"
                )
                .AddField("Top Channels", userStats
                    .GroupBy(x => x.ChannelId)
                    .Select(x => new {Id = x.Key, MessageCount = x.Sum(x => x.MessageCount)})
                    .OrderByDescending(x => x.MessageCount)
                    .Take(5)
                    .Aggregate("", (currentString, nextChannel)
                        => currentString + Environment.NewLine +
                           $"{MentionUtils.MentionChannel(nextChannel.Id)} - {nextChannel.MessageCount} messages"));

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("")]
        public async Task ChannelStatsAsync(ITextChannel channel)
        {
            var totalMessageCount = await StatsUtility.GetTotalMessageCount(Config);
            var channelStats = await StatsUtility.GetMessages(Config, x => x.ChannelId == channel.Id);

            var embedBuilder = new EmbedBuilder
                {
                    Title = $"#{channel.Name} Stats",
                    Color = MomentumColor.Blue
                }.AddField("Total Messages",
                    $"{channelStats.Sum(x => x.MessageCount)} - {(decimal) channelStats.Sum(x => x.MessageCount) / totalMessageCount:P} of total")
                .AddField("Top Users", channelStats
                    .GroupBy(x => x.UserId)
                    .Select(x => new {Id = x.Key, MessageCount = x.Sum(x => x.MessageCount)})
                    .OrderByDescending(x => x.MessageCount)
                    .Take(5)
                    .Aggregate("", (currentString, nextUser)
                        => currentString + Environment.NewLine +
                           $"{MentionUtils.MentionUser(nextUser.Id)} - {nextUser.MessageCount} messages"));

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}