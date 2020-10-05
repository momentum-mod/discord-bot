using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Models.Data;

namespace MomentumDiscordBot.Utilities
{
    public static class StatsUtility
    {
        public static async Task<List<(T Grouping, long MessageCount)>> GetTopMessages<T>(Configuration config,
            Func<DailyMessageCount, T> groupFunc)
        {
            await using var dbContext = DbContextHelper.GetNewDbContext(config);

            return dbContext.DailyMessageCount.ToList().GroupBy(groupFunc)
                .Select(x => (Grouping: x.Key, MessageCount: x.ToList().Sum(x => x.MessageCount)))
                .OrderByDescending(x => x.MessageCount)
                .Take(10)
                .ToList();
        }

        public static DiscordEmbedBuilder GetTopStatsEmbedBuilder<T>(
            this List<(T Grouping, long MessageCount)> topStats,
            string title,
            Func<(T Grouping, long MessageCount), string> elementStringConverterFunc) =>
            new DiscordEmbedBuilder
            {
                Title = title,
                Description = string.Join(Environment.NewLine,
                    topStats.Select(elementStringConverterFunc)),
                Color = MomentumColor.Blue
            };

        public static async Task<List<DailyMessageCount>> GetMessages(Configuration config,
            Func<DailyMessageCount, bool> whereFunc)
        {
            await using var dbContext = DbContextHelper.GetNewDbContext(config);

            return dbContext.DailyMessageCount
                .ToList()
                .Where(whereFunc)
                .ToList();
        }

        public static async Task<long> GetTotalMessageCount(Configuration config)
        {
            await using var dbContext = DbContextHelper.GetNewDbContext(config);

            return dbContext.DailyMessageCount.Sum(x => x.MessageCount);
        }
    }
}