using Microsoft.EntityFrameworkCore;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Models.Data;

namespace MomentumDiscordBot.Utilities
{
    public static class DbContextHelper
    {
        public static MomentumDiscordDbContext GetNewDbContext(Configuration config) =>
            new(new DbContextOptionsBuilder<MomentumDiscordDbContext>()
                .UseMySql(config.MySqlConnectionString, ServerVersion.AutoDetect(config.MySqlConnectionString)).Options);
    }
}