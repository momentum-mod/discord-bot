using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MomentumDiscordBot.Constants;

namespace MomentumDiscordBot.Services
{
    public class LogService
    {
        private readonly object _lock = new object();

        // Register Discord client log events to the service
        public LogService(DiscordSocketClient discordClient) => discordClient.Log += LogAsync;

        internal async Task LogAsync(LogMessage logMessage)
        {
            await Task.Run(() =>
            {
                // Lock so that messages don't get mixed
                lock (_lock)
                {
                    // Give null values an empty value
                    if (logMessage.Message == null)
                        logMessage = new LogMessage(logMessage.Severity, logMessage.Source, string.Empty,
                            logMessage.Exception);
                    if (logMessage.Source == null)
                        logMessage = new LogMessage(logMessage.Severity, string.Empty, logMessage.Message,
                            logMessage.Exception);

                    // Write to the console in an intuitive color
                    Console.ForegroundColor = logMessage.Severity switch
                    {
                        LogSeverity.Critical => ColorConstants.ErrorLogColor,
                        LogSeverity.Error => ColorConstants.ErrorLogColor,
                        LogSeverity.Warning => ColorConstants.WarningLogColor,
                        LogSeverity.Info => ColorConstants.InfoLogColor,
                        LogSeverity.Verbose => ColorConstants.InfoLogColor,
                        LogSeverity.Debug => ColorConstants.InfoLogColor,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    Console.WriteLine(
                        $"[{DateTime.Now:dd/MM HH:mm}] {logMessage.Severity}: <{logMessage.Source}> {logMessage.Message}");

                    if (logMessage.Exception != null)
                    {
                        Console.WriteLine();
                        Console.WriteLine(logMessage.Exception);
                        Console.WriteLine();
                    }

                    // Set default font color back to info
                    Console.ForegroundColor = ColorConstants.InfoLogColor;
                }
            });
        }

        internal async Task LogInfoAsync(string source, string message) => await LogAsync(new LogMessage(
            LogSeverity.Info, source,
            message));

        internal async Task LogError(string source, string message) => await LogAsync(new LogMessage(LogSeverity.Error,
            source,
            message));

        internal async Task LogWarning(string source, string message) => await LogAsync(new LogMessage(
            LogSeverity.Warning,
            source, message));
    }
}