using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Models.Data;
using Serilog;

namespace MomentumDiscordBot.Services
{
    public class UserTrustService
    {
        private ILogger _logger;
        private DiscordSocketClient _discordClient;
        private Config _config;
        public UserTrustService(ILogger logger, DiscordSocketClient discordClient, Config config)
        {
            _logger = logger;
            _discordClient = discordClient;
            _config = config;

            _discordClient.MessageReceived += MessageReceived;
        }

        private Task MessageReceived(SocketMessage message)
        {
            // Ignore bots
            if (message.Author.IsBot)
            {
                return Task.CompletedTask;
            }

            _ = Task.Run(() =>
            {
                using var dbContext = new MomentumDiscordDbContext(new DbContextOptionsBuilder<MomentumDiscordDbContext>().UseMySql(_config.MySqlConnectionString).Options);
                LogMessageCount(dbContext, message);
            });

            return Task.CompletedTask;
        }

        private void LogMessageCount(MomentumDiscordDbContext dbContext, SocketMessage message)
        {
            var users = dbContext.DailyMessageCount.ToList();
            var user = dbContext.DailyMessageCount
                .SingleOrDefault(x => x.UserId == message.Author.Id &&
                                      x.ChannelId == message.Channel.Id &&
                                      x.Date == DateTime.UtcNow.Date);

            if (user != null)
            {
                // If they have a message count for that day, just increment
                user.MessageCount++;
            }
            else
            {
                // No data for the current state, make a new message count
                var newUser = new DailyMessageCount
                {
                    ChannelId = message.Channel.Id,
                    Date = DateTime.UtcNow.Date,
                    UserId = message.Author.Id,
                    MessageCount = 1
                };

                dbContext.Add(newUser);
            }

            dbContext.SaveChanges();
        }
    }
}
