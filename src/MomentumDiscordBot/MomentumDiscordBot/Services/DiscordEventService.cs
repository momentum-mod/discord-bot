using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;
using Serilog;

namespace MomentumDiscordBot.Services
{
    public class DiscordEventService
    {
        private readonly Config _config;
        private readonly DiscordSocketClient _discordClient;
        private IChannel _joinLogChannel;
        private readonly ILogger _logger;

        public DiscordEventService(DiscordSocketClient discordClient, Config config, ILogger logger)
        {
            _discordClient = discordClient;
            _config = config;
            _logger = logger;

            _discordClient.UserJoined += UserJoined;
            _discordClient.Log += Log;
        }

        public Task Log(LogMessage logMessage)
        {
            if (logMessage.Exception is GatewayReconnectException)
            {
                _logger.Information(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message ?? logMessage.Exception?.Message);
                return Task.CompletedTask;
            }

            switch (logMessage.Severity)
            {
                case LogSeverity.Critical:
                    _logger.Fatal(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message ?? logMessage.Exception?.Message);
                    break;
                case LogSeverity.Error:
                    _logger.Error(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message ?? logMessage.Exception?.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.Warning(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message ?? logMessage.Exception?.Message);
                    break;
                case LogSeverity.Info:
                    _logger.Information(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message ?? logMessage.Exception?.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.Verbose(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message ?? logMessage.Exception?.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.Debug(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message ?? logMessage.Exception?.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }

        private Task UserJoined(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                if (_config.JoinLogChannel != default)
                {
                    _joinLogChannel ??= _discordClient.GetChannel(_config.JoinLogChannel);

                    if (_joinLogChannel != null && _joinLogChannel is SocketTextChannel channel)
                    {
                        var accountAge = DateTimeOffset.UtcNow - user.CreatedAt;

                        var userJoinedMessage = await channel.SendMessageAsync(
                            $"{user.Mention} {Format.Sanitize(user.Username.RemoveControlChars())}#{user.Discriminator} joined, account was created {accountAge.ToPrettyFormat()} ago");

                        if (accountAge.TotalHours <= 24)
                        {
                            await userJoinedMessage.AddReactionAsync(Emote.Parse(_config.NewUserEmoteString));
                        }
                        else
                        {
                            var messages = await channel.GetMessagesAsync(200).FlattenAsync();

                            // Find a matching user in the recent history
                            var altAccount = messages
                                .FromSelf(_discordClient)
                                .OrderByDescending(x => x.Timestamp)
                                // Parse the username from the bot's message, and make sure it has the new user emote
                                .FirstOrDefault(x => x.Reactions.ContainsKey(Emote.Parse(_config.NewUserEmoteString)) &&
                                                     x.Content.Split(' ', 3)[1].Split('#')[0] == user.Username);

                            // Is there a matching account
                            if (altAccount != null)
                            {
                                await altAccount.RemoveAllReactionsForEmoteAsync(
                                    Emote.Parse(_config.NewUserEmoteString));
                                await altAccount.AddReactionAsync(_config.AltAccountEmoji);
                            }
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}