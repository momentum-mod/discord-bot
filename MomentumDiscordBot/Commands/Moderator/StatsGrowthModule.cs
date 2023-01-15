using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Models.Data;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Commands.Moderator
{
    [SlashCommandGroup("growth", "Compare message count")]
    public class StatsGrowthModule : ModeratorModuleBase
    {
        public Configuration Config { get; set; }

        [SlashCommand("user", "Compares a user's message count in two groups of 30 day periods")]
        public async Task UserStatsAsync(InteractionContext context, [Option("member", "member")] DiscordUser user)
        {
            await context.DeferAsync();

            var member = (DiscordMember)user;

            var userStats = await StatsUtility.GetMessages(Config, x => x.UserId == member.Id);

            await context.EditResponseAsync(
                new DiscordWebhookBuilder().AddEmbed(GetGrowthEmbed(userStats, member.Mention)));
        }

        [SlashCommand("channel", "Compares a channels's message count in two groups of 30 day periods")]
        public async Task ChannelStatsAsync(InteractionContext context, [Option("channel", "channel")] DiscordChannel channel)
        {
            await context.DeferAsync();

            if (channel.Type != ChannelType.Text)
            {
                await ReplyNewEmbedAsync(context, "Channel must be a text channel.", DiscordColor.Orange);
                return;
            }

            var channelStats = await StatsUtility.GetMessages(Config, x => x.ChannelId == channel.Id);

            await context.EditResponseAsync(
                new DiscordWebhookBuilder().AddEmbed(GetGrowthEmbed(channelStats, channel.Mention)));
        }

        private static DiscordEmbedBuilder GetGrowthEmbed(List<DailyMessageCount> filteredMessages, string mention)
        {
            // Filters the past 30 days
            var thisMonthMessages = filteredMessages.Where(x => x.Date.Ticks > DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0, 0)).Ticks)
                .Aggregate((long)0, (totalCount, nextCount) => totalCount + nextCount.MessageCount);

            // Filters the past 60 days, but not the past 30
            var lastMonthMessages = filteredMessages.Where(x => x.Date.Ticks < DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0, 0)).Ticks
                                                             && x.Date.Ticks > DateTime.UtcNow.Subtract(new TimeSpan(60, 0, 0, 0)).Ticks)
                .Aggregate((long)0, (totalCount, nextCount) => totalCount + nextCount.MessageCount);

            if (lastMonthMessages == 0)
            {
                // Can't divide by zero
                return new DiscordEmbedBuilder
                {
                    Description = "No data from last month to compare",
                    Color = DiscordColor.Orange
                };
            }

            if (thisMonthMessages == 0)
            {
                return new DiscordEmbedBuilder
                {
                    Description = "No data from this month to compare",
                    Color = DiscordColor.Orange
                };
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

            return new DiscordEmbedBuilder
            {
                Description = $"{mention}'s activity has {deltaText} ({thisMonthMessages} vs {lastMonthMessages})",
                Color = MomentumColor.Blue
            };
        }
    }
}
