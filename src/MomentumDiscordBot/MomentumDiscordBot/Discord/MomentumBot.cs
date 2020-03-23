using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;
using TwitchLib.Api.Helix.Models.Streams;

namespace MomentumDiscordBot.Discord
{
    public class MomentumBot
    {
        private readonly Config _config;
        private readonly DiscordSocketClient _discordClient;
        private readonly string _discordToken;
        private readonly LogService _logService;
        private readonly DependencyInjectionService _dependencyInjectionService;
        private readonly MomentumCommandService _momentumCommandService;
        private ReactionBasedRoleService _reactionBasedRoleService;
        private readonly IServiceProvider _services;
        private StreamMonitorService _streamMonitorService;

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
            _discordClient.Ready += _discordClient_Ready;

            var baseCommandService = MomentumCommandService.BuildBaseCommandService();
            _streamMonitorService = new StreamMonitorService(_discordClient, _config);
            _dependencyInjectionService = new DependencyInjectionService(baseCommandService, _discordClient, config);
            _services = _dependencyInjectionService.BuildServiceProvider(_streamMonitorService);

            var logger = _services.GetRequiredService<LogService>();
            _momentumCommandService =
                new MomentumCommandService(_discordClient, baseCommandService, logger, config, _services);
        }

        private Task _discordClient_Ready()
        {
            // Start updating streams
            _streamMonitorService.Start();
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