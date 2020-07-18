using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;
using Serilog;
using Serilog.Events;

namespace MomentumDiscordBot.Discord
{
    public class MomentumBot
    {
        private readonly Config _config;
        private readonly DiscordSocketClient _discordClient;
        private readonly string _discordToken;
        private readonly MomentumCommandService _momentumCommandService;
        private readonly StreamMonitorService _streamMonitorService;
        private readonly ILogger _logger;
        private readonly DeadlockWorkaroundService _deadlockWorkaroundService;
        public MomentumBot(string discordToken, Config config, ILogger logger)
        {
            _discordToken = discordToken;
            _config = config;
            _logger = logger;

            var discordClientConfig = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 200
            };

            _discordClient = new DiscordSocketClient(discordClientConfig);

            var baseCommandService = MomentumCommandService.BuildBaseCommandService();

            _streamMonitorService = new StreamMonitorService(_discordClient, _config, _logger);

            var services = BuildServiceProvider(baseCommandService);

            _ = services.GetRequiredService<FaqService>();
            var discordEventService = services.GetRequiredService<DiscordEventService>();
            _ = services.GetRequiredService<MessageHistoryService>();
            _ = services.GetRequiredService<ReactionBasedRoleService>();
            _ = services.GetRequiredService<KeyBeggingService>();
          
            _momentumCommandService =
                new MomentumCommandService(_discordClient, baseCommandService, _logger, config, services);

            _deadlockWorkaroundService = new DeadlockWorkaroundService(_discordClient, discordEventService.Log);
        }

        public IServiceProvider BuildServiceProvider(CommandService baseCommandService) =>
            new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(_logger)
                .AddSingleton(baseCommandService)
                .AddSingleton(_discordClient)
                .AddSingleton<ReactionBasedRoleService>()
                .AddSingleton(_streamMonitorService)
                .AddSingleton<DiscordEventService>()
                .AddSingleton<MessageHistoryService>()
                .AddSingleton<FaqService>()
                .AddSingleton<KeyBeggingService>()
                .BuildServiceProvider();

        internal async Task<Exception> RunAsync()
        {
            try
            {
                // Register commands
                await _momentumCommandService.InitializeAsync();

                // Login and start bot
                await _discordClient.LoginAsync(TokenType.Bot, _discordToken);
                await _discordClient.StartAsync();

                // Block the task indefinitely
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                return e;
            }

            return null;
        }
    }
}