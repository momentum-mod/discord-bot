using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Commands.Moderator
{
    [SlashCommandGroup("stats", "stats commands")]
    public class StatsModule : ModeratorModuleBase
    {
        public Configuration Config { get; set; }

        [SlashCommand("user", "show user stats")]
        public async Task UserStatsAsync(InteractionContext context, [Option("member", "member")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            var totalMessageCount = await StatsUtility.GetTotalMessageCount(Config);
            var userStats = await StatsUtility.GetMessages(Config, x => x.UserId == member.Id);

            var embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "User Stats",
                    Color = MomentumColor.Blue
                }.WithAuthor(member.DisplayName, iconUrl: member.AvatarUrl ?? member.DefaultAvatarUrl)
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
                           $"{context.Client.FindChannel(nextChannel.Id).Mention} - {nextChannel.MessageCount} messages"));

            await context.CreateResponseAsync(embed: embedBuilder.Build());
        }

        [SlashCommand("channel", "show channel stats")]
        public async Task ChannelStatsAsync(InteractionContext context, [Option("channel", "channel")] DiscordChannel channel)
        {
            if (channel.Type != ChannelType.Text)
            {
                await ReplyNewEmbedAsync(context, "Channel must be a text channel.", DiscordColor.Orange);
                return;
            }

            var totalMessageCount = await StatsUtility.GetTotalMessageCount(Config);
            var channelStats = await StatsUtility.GetMessages(Config, x => x.ChannelId == channel.Id);

            var embedBuilder = new DiscordEmbedBuilder
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
                           $"{context.Guild.Members.Values.FirstOrDefault(x => x.Id == nextUser.Id)?.Mention ?? nextUser.Id.ToString()} - {nextUser.MessageCount} messages"));

            await context.CreateResponseAsync(embed: embedBuilder.Build());
        }
    }
}
