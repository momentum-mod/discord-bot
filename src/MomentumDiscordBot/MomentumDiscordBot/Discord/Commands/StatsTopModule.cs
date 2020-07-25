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
            await using var dbContext = DbContextHelper.GetNewDbContext(Config);

            var topUsers = dbContext.DailyMessageCount.ToList().GroupBy(x => x.UserId)
                .Select(x => new KeyValuePair<ulong, long>(x.Key, x.ToList().Sum(x => x.MessageCount)))
                .OrderByDescending(x => x.Value);

            var embedBuilder = new EmbedBuilder
            {
                Title = "Most Active Users",
                Description = string.Join(Environment.NewLine,
                    topUsers.Select(x => MentionUtils.MentionUser(x.Key) + " - " + x.Value + " messages")),
                Color = MomentumColor.Blue
            };

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}