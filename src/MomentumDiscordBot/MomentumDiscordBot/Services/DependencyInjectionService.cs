using System;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Services
{
    public class DependencyInjectionService
    {
        private readonly CommandService _baseCommandService;
        private readonly Config _config;
        private readonly DiscordSocketClient _discordClient;

        public DependencyInjectionService(CommandService baseCommandService, DiscordSocketClient discordClient,
            Config config)
        {
            _baseCommandService = baseCommandService;
            _discordClient = discordClient;
            _config = config;
        }

        public IServiceProvider BuildServiceProvider(StreamMonitorService streamMonitorService) =>
            new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<LogService>()
                .AddSingleton(_baseCommandService)
                .AddSingleton(_discordClient)
                .AddSingleton<ReactionBasedRoleService>()
                .AddSingleton(streamMonitorService)
                .AddSingleton<FaqService>()
                .BuildServiceProvider();
    }
}