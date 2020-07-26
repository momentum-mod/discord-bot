using Microsoft.EntityFrameworkCore;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Models.Data;

namespace MomentumDiscordBot.Utilities
{
    public static class DbContextHelper
    {
        public static MomentumDiscordDbContext GetNewDbContext(Config config) =>
            new MomentumDiscordDbContext(new DbContextOptionsBuilder<MomentumDiscordDbContext>()
                .UseMySql(config.MySqlConnectionString).Options);
    }
}