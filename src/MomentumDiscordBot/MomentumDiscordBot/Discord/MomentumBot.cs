using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord
{
    public class MomentumBot
    {
        private readonly string _discordToken;
        private readonly DiscordSocketClient _discordClient;
        private readonly LogService _logService;
        private StreamMonitorService _streamMonitorService;
        private readonly Config _config;
        private DependencyInjectionService _dependencyInjectionService;
        private IServiceProvider _services;
        private MomentumCommandService _momentumCommandService;
        private ReactionBasedRoleService _reactionBasedRoleService;

        public MomentumBot(string discordToken, Config config)
        {
            _discordToken = discordToken;
            _config = config;

            var discordClientConfig = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false,
                MessageCacheSize = 200
            };

            _discordClient = new DiscordSocketClient(discordClientConfig);
            _discordClient.Ready += _discordClient_Ready;

            var baseCommandService = MomentumCommandService.BuildBaseCommandService();
            _dependencyInjectionService = new DependencyInjectionService(baseCommandService, _discordClient, config);
            _services = _dependencyInjectionService.BuildServiceProvider();

            var logger = _services.GetRequiredService<LogService>();
            _momentumCommandService = new MomentumCommandService(_discordClient, baseCommandService, logger, config, _services);
        }

        private Task _discordClient_Ready()
        {
            // Start updating streams
            _streamMonitorService = new StreamMonitorService(_discordClient, TimeSpan.FromMinutes(5), _config.MomentumModStreamerChannelId);
            _reactionBasedRoleService = _services.GetRequiredService<ReactionBasedRoleService>();

            return Task.CompletedTask;
        }

        internal async Task<Exception> RunAsync()
        {
            try
            {
                // Register commands
                await _momentumCommandService.InitializeAsync();

                // Login and start bot
                await _discordClient.LoginAsync(TokenType.Bot, _discordToken, validateToken: true);
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