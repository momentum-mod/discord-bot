using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Services
{
    public class MomentumCommandService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _baseCommandService;
        private readonly IServiceProvider _serviceProvider;
        private readonly LogService _logService;
        private readonly Config _config;
        public MomentumCommandService(DiscordSocketClient discordClient, CommandService baseCommandService, IServiceProvider serviceProvider, LogService logService, Config config)
        {
            // Parameters are injected
            _discordClient = discordClient;
            _baseCommandService = baseCommandService;
            _serviceProvider = serviceProvider;
            _logService = logService;
            _config = config;
        }

        internal async Task HandleCommandAsync(SocketMessage inputMessage)
        {
            // Don't process the command if it was a system message
            if (inputMessage is SocketUserMessage message)
            {
                // Create a number to track where the prefix ends and the command begins
                var argPosition = 0;

                // Determine if the message is a command based on the prefix and make sure no bots trigger commands
                if (!(message.HasStringPrefix(_config.CommandPrefix, ref argPosition) ||
                      message.HasMentionPrefix(_discordClient.CurrentUser, ref argPosition)) ||
                    message.Author.IsBot) return;

                // Create a WebSocket-based command context based on the message
                var context = new SocketCommandContext(_discordClient, message);

                // Execute the command with the command context we just
                // created, along with the service provider for precondition checks.
                await _baseCommandService.ExecuteAsync(
                    context: context,
                    argPos: argPosition,
                    services: _serviceProvider);
            }
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
