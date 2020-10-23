using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Commands.Moderator
{
    [Group("growth")]
    public class StatsGrowthModule : ModeratorModuleBase
    {
        public Configuration Config { get; set; }

        [Command("user")]
        public async Task UserStatsAsync(CommandContext context, DiscordMember member)
        {
            var userStats = await StatsUtility.GetMessages(Config, x => x.UserId == member.Id);

            var thisMonthMessages = userStats.Where(x => x.Date.Month == DateTime.UtcNow.Month)
                .Aggregate((long)0, (totalCount, nextCount) => totalCount + nextCount.MessageCount);

            var lastMonthMessages = userStats.Where(x => x.Date.Month == DateTime.UtcNow.Month - 1)
                .Aggregate((long)0, (totalCount, nextCount) => totalCount + nextCount.MessageCount);

            if (lastMonthMessages == 0)
            {
                // Can't divide by zero
                await ReplyNewEmbedAsync(context, "No data from last month to compare", DiscordColor.Orange);
                return;
            }

            if (thisMonthMessages == 0)
            {
                await ReplyNewEmbedAsync(context, "No data from this month to compare", DiscordColor.Orange);
                return;
            }

            var delta = (decimal) thisMonthMessages / lastMonthMessages;

            string deltaText;
            if (delta == 1)
            {
                deltaText = "stayed exactly the same as last month";
            }
            else if (delta < 1)
            {
                deltaText = $"gone down by {1 - delta:P2} this month";
            }
            else
            {
                deltaText = $"gone up by {delta - 1:P2} this month";
            }

            await ReplyNewEmbedAsync(context, $"{member.Mention}'s activity has {deltaText}", MomentumColor.Blue);
        }

        [Command("channel")]
        public async Task ChannelStatsAsync(CommandContext context, DiscordChannel channel)
        {
            if (channel.Type != ChannelType.Text)
            {
                await ReplyNewEmbedAsync(context, "Channel must be a text channel.", DiscordColor.Orange);
                return;
            }

            var channelStats = await StatsUtility.GetMessages(Config, x => x.ChannelId == channel.Id);

            var thisMonthMessages = channelStats.Where(x => x.Date.Month == DateTime.UtcNow.Month)
                .Aggregate((long)0, (totalCount, nextCount) => totalCount + nextCount.MessageCount);

            var lastMonthMessages = channelStats.Where(x => x.Date.Month == DateTime.UtcNow.Month - 1)
                .Aggregate((long)0, (totalCount, nextCount) => totalCount + nextCount.MessageCount);

            if (lastMonthMessages == 0)
            {
                // Can't divide by zero
                await ReplyNewEmbedAsync(context, "No data from last month to compare", DiscordColor.Orange);
                return;
            }

            if (thisMonthMessages == 0)
            {
                await ReplyNewEmbedAsync(context, "No data from this month to compare", DiscordColor.Orange);
                return;
            }

            var delta = (decimal)thisMonthMessages / lastMonthMessages;

            string deltaText;
            if (delta == 1)
            {
                deltaText = "stayed exactly the same as last month";
            }
            else if (delta < 1)
            {
                deltaText = $"gone down by {1 - delta:P2} this month";
            }
            else
            {
                deltaText = $"gone up by {delta - 1:P2} this month";
            }

            await ReplyNewEmbedAsync(context, $"{channel.Mention}'s activity has {deltaText}", MomentumColor.Blue);
        }
    }
}
