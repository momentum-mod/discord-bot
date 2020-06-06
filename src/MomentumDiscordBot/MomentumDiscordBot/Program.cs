using System;
using System.IO;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Discord;
using MomentumDiscordBot.Models;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace MomentumDiscordBot
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Loading discord token...");
            if (!TryGetDiscordToken(out var discordToken))
                // No token, quit
                return;

            Console.WriteLine("Loading config file...");
            var config = Config.LoadFromFile();

            using var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .WriteTo.Seq(config.SeqAddress, LogEventLevel.Warning, apiKey: config.SeqToken)
                .Enrich.WithProperty("Environment", config.Environment)
                .Enrich.WithProperty("Application", "Discord Bot")
                .CreateLogger();

            var bot = new MomentumBot(discordToken, config, logger);

            var closingException = bot.RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            // Log unsafe exits, have to do this explicitly in async context
            if (closingException == null)
            {
                Console.WriteLine("Application quitting safely.");
            }
            else
            {
                logger.Error(closingException, "Application quitting unsafely");
            }
        }

        private static bool TryGetDiscordToken(out string discordToken)
        {
            if (File.Exists(PathConstants.DiscordTokenFilePath))
            {
                // File exists, get the text
                discordToken = File.ReadAllText(PathConstants.DiscordTokenFilePath);
                return true;
            }

            Console.WriteLine(
                $"No discord token file exists, expected it at: '{PathConstants.DiscordTokenFilePath}'");

            discordToken = null;
            return false;
        }
    }
}