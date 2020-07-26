using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Models.Data;
using MomentumDiscordBot.Utilities;
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

            _ = Task.Run(async () =>
            {
                await using var dbContext = DbContextHelper.GetNewDbContext(_config);
                LogMessageCount(dbContext, message);
                await CheckVerifiedRoleAsync(dbContext, message);
            });

            return Task.CompletedTask;
        }

        private void LogMessageCount(MomentumDiscordDbContext dbContext, SocketMessage message)
        {
            var users = dbContext.DailyMessageCount.ToList();
            var user = dbContext.DailyMessageCount
                .SingleOrDefault(x => x.UserId == message.Author.Id &&
                                      x.ChannelId == message.Channel.Id &&
                                      x.Date == message.CreatedAt.UtcDateTime.Date);

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
                    Date = message.CreatedAt.UtcDateTime.Date,
                    UserId = message.Author.Id,
                    MessageCount = 1
                };

                dbContext.Add(newUser);
            }

            dbContext.SaveChanges();
        }

        private async Task CheckVerifiedRoleAsync(MomentumDiscordDbContext dbContext, SocketMessage message)
        {
            // If they already have the verified role, or they have the blacklist role, no need to check
            if (message.Author is IGuildUser guildUser && !guildUser.RoleIds.Any(x => x == _config.MediaVerifiedRoleId || x == _config.MediaBlacklistedRoleId))
            {
                // Have they been here for the minimum days
                var messagesFromUser = dbContext.DailyMessageCount.ToList()
                    .Where(x => x.UserId == guildUser.Id)
                    .OrderBy(x => x.Date)
                    .ToList();

                if (!messagesFromUser.Any())
                {
                    // Haven't sent a message
                    return;
                }

                var earliestMessage = messagesFromUser.FirstOrDefault();

                if ((DateTime.UtcNow - earliestMessage.Date).TotalDays > _config.MediaMinimumDays)
                {
                    // They have been here minimum days, sum messages
                    var messageCount = messagesFromUser.Sum(x => x.MessageCount);

                    if (messageCount > _config.MediaMinimumMessages)
                    {
                        // User meets all the requirements
                        var verifiedRole = guildUser.Guild.GetRole(_config.MediaVerifiedRoleId);

                        await guildUser.AddRoleAsync(verifiedRole);
                    }
                }
            }
        }
    }
}
