using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Models.Data;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Services
{
    [Microservice(MicroserviceType.InjectAndInitialize)]
    public class UserTrustService
    {
        private readonly Configuration _config;
        private readonly DiscordClient _discordClient;

        public UserTrustService(DiscordClient discordClient, Configuration config)
        {
            _discordClient = discordClient;
            _config = config;

            _discordClient.MessageCreated += DiscordClient_MessageCreated;
        }

        private Task DiscordClient_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                // Ignore bots or DMs 
                if (e.Author.IsBot || e.Channel.IsPrivate)
                {
                    return;
                }

                await using var dbContext = DbContextHelper.GetNewDbContext(_config);
                LogMessageCount(dbContext, e.Message);
                await CheckVerifiedRoleAsync(dbContext, e.Message);
            });

            return Task.CompletedTask;
        }

        private static void LogMessageCount(MomentumDiscordDbContext dbContext, DiscordMessage message)
        {
            var user = dbContext.DailyMessageCount
                .SingleOrDefault(x => x.UserId == message.Author.Id &&
                                      x.ChannelId == message.Channel.Id &&
                                      x.Date == message.CreationTimestamp.UtcDateTime.Date);

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
                    Date = message.CreationTimestamp.UtcDateTime.Date,
                    UserId = message.Author.Id,
                    MessageCount = 1
                };

                dbContext.Add(newUser);
            }

            dbContext.SaveChanges();
        }

        private async Task CheckVerifiedRoleAsync(MomentumDiscordDbContext dbContext, DiscordMessage message)
        {
            // If they already have the verified role, or they have the blacklist role, no need to check
            if (message.Author is not DiscordMember member)
            {
                return;
            }

            if (!member.Roles.Any(x => x.Id == _config.MediaVerifiedRoleId || x.Id == _config.MediaBlacklistedRoleId))
            {
                // Have they been here for the minimum days
                var messagesFromUser = dbContext.DailyMessageCount.ToList()
                    .Where(x => x.UserId == member.Id)
                    .OrderBy(x => x.Date)
                    .ToList();

                if (!messagesFromUser.Any())
                {
                    // Haven't sent a message
                    return;
                }

                // Has to be at least one message, because this runs after message received event
                var earliestMessage = messagesFromUser.First();

                if ((DateTime.UtcNow - earliestMessage.Date).TotalDays > _config.MediaMinimumDays)
                {
                    // They have been here minimum days, sum messages
                    var messageCount = messagesFromUser.Sum(x => x.MessageCount);

                    if (messageCount > _config.MediaMinimumMessages)
                    {
                        // User meets all the requirements
                        var verifiedRole = member.Guild.GetRole(_config.MediaVerifiedRoleId);

                        await member.GrantRoleAsync(verifiedRole);
                    }
                }
            }
        }
    }
}