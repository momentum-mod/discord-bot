using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Services
{
    [Microservice(MicroserviceType.InjectAndInitialize)]
    public class DiscordEventService
    {
        private readonly Configuration _config;
        private readonly DiscordClient _discordClient;
        private DiscordChannel _joinLogChannel;

        public DiscordEventService(DiscordClient discordClient, Configuration config)
        {
            _discordClient = discordClient;
            _config = config;

            _discordClient.GuildDownloadCompleted += DiscordClient_GuildDownloadCompleted;
            _discordClient.GuildMemberAdded += DiscordClient_UserJoined;
        }

        private Task DiscordClient_GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                foreach (var (_, guild) in _discordClient.Guilds)
                {
                    await guild.RequestMembersAsync(presences: true, nonce: Environment.TickCount.ToString());
                }
            });

            return Task.CompletedTask;
        }

        private Task DiscordClient_UserJoined(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                // Haven't set the config
                if (_config.JoinLogChannel == default)
                {
                    return;
                }

                _joinLogChannel ??= _discordClient.FindChannel(_config.JoinLogChannel);

                // Invalid channel ID
                if (_joinLogChannel == null)
                {
                    return;
                }

                var accountAge = DateTimeOffset.UtcNow - e.Member.CreationTimestamp.UtcDateTime;
                var userJoinedMessage = await _joinLogChannel.SendMessageAsync(
                    $"{e.Member.Mention} {Formatter.Sanitize(e.Member.Username.RemoveControlChars())}#{e.Member.Discriminator} joined, account was created {accountAge.ToPrettyFormat()} ago");

                await WarnIfNewAccountAsync(userJoinedMessage, accountAge);
                await WarnIfDuplicatedNewAccountAsync(e.Member);
            });

            return Task.CompletedTask;
        }

        private async Task WarnIfNewAccountAsync(DiscordMessage userJoinedMessage, TimeSpan accountAge)
        {
            if (accountAge.TotalHours <= 24)
            {
                // New user, add the emoji warning
                await userJoinedMessage.CreateReactionAsync(DiscordEmoji.FromName(_discordClient,
                    _config.NewUserEmoteString));
            }
        }

        private async Task WarnIfDuplicatedNewAccountAsync(DiscordUser member)
        {
            var messages = await _joinLogChannel.GetMessagesAsync();

            // Find a matching user in the recent history
            var altAccount = messages
                .FromSelf(_discordClient)
                .OrderByDescending(x => x.Timestamp)
                // Parse the username from the bot's message, and make sure it has the new user emote
                .FirstOrDefault(x =>
                    x.Reactions.Any(x =>
                        x.Emoji == DiscordEmoji.FromName(_discordClient, _config.NewUserEmoteString)) &&
                    x.Content.Split(' ', 3)[1].Split('#')[0] == member.Username);

            // Is there a matching account
            if (altAccount != null)
            {
                await altAccount.DeleteReactionsEmojiAsync(
                    DiscordEmoji.FromName(_discordClient, _config.NewUserEmoteString));
                await altAccount.CreateReactionAsync(_config.AltAccountEmoji);
            }
        }
    }
}