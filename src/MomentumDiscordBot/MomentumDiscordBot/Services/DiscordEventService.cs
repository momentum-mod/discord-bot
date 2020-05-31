using System;
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
        private readonly DiscordSocketClient _discordClient;
        private readonly Config _config;
        private IChannel _joinLogChannel;
        private ILogger _logger;
        public DiscordEventService(DiscordSocketClient discordClient, Config config, ILogger logger)
        {
            _discordClient = discordClient;
            _config = config;
            _logger = logger;

            _discordClient.UserJoined += UserJoined;
            _discordClient.Log += Log;
        }
        private Task Log(LogMessage logMessage)
        {
            switch (logMessage.Severity)
            {
                case LogSeverity.Critical:
                    _logger.Fatal(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message);
                    break;
                case LogSeverity.Error:
                    _logger.Error(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.Warning(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message);
                    break;
                case LogSeverity.Info:
                    _logger.Information(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.Verbose(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.Debug(logMessage.Exception, "{Source}: {Message}", logMessage.Source, logMessage.Message);
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
                            $"{user.Mention} {user.Username}#{user.Discriminator} joined, account was created {accountAge.ToPrettyFormat()} ago");

                        if (accountAge.TotalHours <= 24)
                        {
                            await userJoinedMessage.AddReactionAsync(Emote.Parse(_config.NewUserEmoteString));
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}
