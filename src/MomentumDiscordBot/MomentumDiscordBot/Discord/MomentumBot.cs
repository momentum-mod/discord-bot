using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Discord.Commands;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord
{
    public class MomentumBot
    {
        private readonly Config _config;
        private readonly DiscordSocketClient _discordClient;
        private readonly string _discordToken;
        private readonly MomentumCommandService _momentumCommandService;
        private readonly StreamMonitorService _streamMonitorService;
        private readonly LogService _logger;

        public MomentumBot(string discordToken, Config config)
        {
            _discordToken = discordToken;
            _config = config;

            var discordClientConfig = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 200
            };

            _discordClient = new DiscordSocketClient(discordClientConfig);

            var baseCommandService = MomentumCommandService.BuildBaseCommandService();
            _logger = new LogService(_discordClient);

            _streamMonitorService = new StreamMonitorService(_discordClient, _config, _logger);
            var services = BuildServiceProvider(baseCommandService);

            _ = services.GetRequiredService<DiscordEventService>();
            _ = services.GetRequiredService<MessageHistoryService>();
            _ = services.GetRequiredService<ReactionBasedRoleService>();
            _ = services.GetRequiredService<KeyBeggingService>();

            _momentumCommandService =
                new MomentumCommandService(_discordClient, baseCommandService, _logger, config, services);
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