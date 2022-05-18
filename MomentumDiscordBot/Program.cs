using System;
using System.Threading.Tasks;
using MomentumDiscordBot.Models;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;

namespace MomentumDiscordBot
{
    public static class Program
    {
        internal static void Main()
            => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            var config = await Configuration.LoadFromFileAsync();

            SelfLog.Enable(Console.WriteLine);

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console();

            if (!string.IsNullOrEmpty(config.SeqAddress))
            {
                loggerConfig
                    .WriteTo.Seq(config.SeqAddress, LogEventLevel.Information, apiKey: config.SeqToken);
            }

            using var logger = loggerConfig
                .Enrich.WithProperty("Environment", config.Environment)
                .Enrich.WithProperty("Application", "Discord Bot")
                .CreateLogger();

            var bot = new Bot(config, logger);
            await bot.StartAsync();

            await Task.Delay(-1);
        }
    }
}
