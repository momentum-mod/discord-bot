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
        private readonly DiscordSocketClient _discordClient;
        private readonly Config _config;
        public DependencyInjectionService(CommandService baseCommandService, DiscordSocketClient discordClient, Config config)
        {
            _baseCommandService = baseCommandService;
            _discordClient = discordClient;
            _config = config;
        }
        public IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<LogService>()
                .AddSingleton(_baseCommandService)
                .AddSingleton(_discordClient)
                .AddSingleton<ReactionBasedRoleService>()
                .BuildServiceProvider();
        }
    }
}