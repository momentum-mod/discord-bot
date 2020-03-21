using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MomentumDiscordBot.Services
{
    public class MomentumCommandService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _baseCommandService;
        private readonly IServiceProvider _serviceProvider;
        private readonly LogService _logService;
        public MomentumCommandService(DiscordSocketClient discordClient, CommandService baseCommandService, IServiceProvider serviceProvider, LogService logService)
        {
            // Parameters are injected
            _discordClient = discordClient;
            _baseCommandService = baseCommandService;
            _serviceProvider = serviceProvider;
            _logService = logService;
        }
        public static CommandService BuildBaseCommandService()
        {
            var commandServiceConfig = new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                // Default RunMode to async to prevent commands from blocking the thread
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = true,
                LogLevel = LogSeverity.Info
            };

            return new CommandService(commandServiceConfig);
        }
    }
}
