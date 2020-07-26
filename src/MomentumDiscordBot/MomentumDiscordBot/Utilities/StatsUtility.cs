using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Models.Data;

namespace MomentumDiscordBot.Utilities
{
    public static class StatsUtility
    {
        public static async Task<List<(T Grouping, long MessageCount)>> GetTopMessages<T>(Config config, Func<DailyMessageCount, T> groupFunc)
        {
            await using var dbContext = DbContextHelper.GetNewDbContext(config);

            return dbContext.DailyMessageCount.ToList().GroupBy(groupFunc)
                .Select(x => (Grouping:x.Key, MessageCount:x.ToList().Sum(x => x.MessageCount)))
                .OrderByDescending(x => x.MessageCount)
                .Take(10)
                .ToList();
        }
    }
}
