using Microsoft.EntityFrameworkCore;

namespace MomentumDiscordBot.Models.Data
{
    public class MomentumDiscordDbContext : DbContext
    {
        public MomentumDiscordDbContext(DbContextOptions<MomentumDiscordDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DailyMessageCount>()
                .HasKey(x => new {x.UserId, x.ChannelId, x.Date});
        }

        public DbSet<DailyMessageCount> DailyMessageCount { get; set; }
    }
}
