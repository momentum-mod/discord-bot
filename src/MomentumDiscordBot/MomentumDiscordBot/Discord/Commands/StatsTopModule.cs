using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Discord.Commands
{
    [Group("stats top")]
    public class StatsTopModule : MomentumModuleBase
    {
        public Config Config { get; set; }

        [Command("users")]
        public async Task TopUsersAsync()
        {
            var topUsers = await StatsUtility.GetTopMessages(Config, x => x.UserId);

            var embedBuilder = new EmbedBuilder
            {
                Title = "Most Active Users",
                Description = string.Join(Environment.NewLine,
                    topUsers.Select(x => MentionUtils.MentionUser(x.Grouping) + " - " + x.MessageCount + " messages")),
                Color = MomentumColor.Blue
            };

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("channels")]
        public async Task TopChannelsAsync()
        {
            var topUsers = await StatsUtility.GetTopMessages(Config, x => x.ChannelId);


            var embedBuilder = new EmbedBuilder
            {
                Title = "Most Active Channels",
                Description = string.Join(Environment.NewLine,
                    topUsers.Select(x => MentionUtils.MentionChannel(x.Grouping) + " - " + x.MessageCount + " messages")),
                Color = MomentumColor.Blue
            };

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}