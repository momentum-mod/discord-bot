using System.IO;
using Microsoft.EntityFrameworkCore;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Models.Data;

namespace MomentumDiscordBot.Utilities
{
    public static class DbContextHelper
    {
        public static MomentumDiscordDbContext GetNewDbContext(Configuration config) =>
            new(new DbContextOptionsBuilder<MomentumDiscordDbContext>()
                .UseSqlite($"Data Source={PathConstants.DbFilePath}").Options);
    }
}