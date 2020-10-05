using System.Threading.Tasks;
using MomentumDiscordBot.Models;
using Serilog;
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

            using var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq(config.SeqAddress, LogEventLevel.Information, apiKey: config.SeqToken)
                .Enrich.WithProperty("Environment", config.Environment)
                .Enrich.WithProperty("Application", "Discord Bot")
                .CreateLogger();

            var bot = new Bot(config, logger);
            await bot.StartAsync();

            await Task.Delay(-1);
        }
    }
}