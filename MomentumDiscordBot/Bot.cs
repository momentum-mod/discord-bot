using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MomentumDiscordBot
{
    public class Bot
    {
        private readonly Configuration _config;
        private readonly DiscordClient _discordClient;
        private readonly ILogger _logger;

        public Bot(Configuration config, ILogger logger)
        {
            _config = config;
            _logger = logger;

            var logFactory = new LoggerFactory().AddSerilog(logger);

            _discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = _config.BotToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.None,
                LoggerFactory = logFactory,
                MessageCacheSize = 512,
                Intents = DiscordIntents.All
            });

            var services = BuildServiceProvider();
            services.InitializeMicroservices(Assembly.GetEntryAssembly());
        }

        private IServiceProvider BuildServiceProvider()
            => new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(_logger)
                .AddSingleton(_discordClient)
                .InjectMicroservices(Assembly.GetEntryAssembly())
                .BuildServiceProvider();

        public async Task StartAsync()
            => await _discordClient.ConnectAsync();
    }
}