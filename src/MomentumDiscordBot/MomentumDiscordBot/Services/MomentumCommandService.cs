using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MomentumDiscordBot.Models;
using Serilog;

namespace MomentumDiscordBot.Services
{
    public class MomentumCommandService
    {
        private readonly CommandService _baseCommandService;
        private readonly Config _config;
        private readonly DiscordSocketClient _discordClient;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public MomentumCommandService(DiscordSocketClient discordClient, CommandService baseCommandService,
            ILogger logger, Config config, IServiceProvider serviceProvider)
        {
            // Parameters are injected
            _discordClient = discordClient;
            _baseCommandService = baseCommandService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;
        }

        internal async Task InitializeAsync()
        {
            // Main handler for command input
            _discordClient.MessageReceived += HandleCommandAsync;
            _logger.Information("CommandService: Registered MessageReceived event");

            // Post execution handler
            _baseCommandService.CommandExecuted += OnCommandExecutedAsync;
            _logger.Information("CommandService: Registered CommandExecuted event");

            // Install discord commands
            await _baseCommandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            _logger.Information($"CommandService: Added {_baseCommandService.Modules.Count()} modules using reflection, with a total of {_baseCommandService.Commands.Count()} commands");
        }

        private async Task HandleCommandAsync(SocketMessage inputMessage)
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
                    context,
                    argPosition,
                    _serviceProvider);
            }
        }

        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context,
            IResult result)
        {
            // Don't respond to unknown commands
            if (result?.Error != null && result.Error == CommandError.UnknownCommand) return;

            // Since commands are run in an async context, errors have to be manually handled
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                var embedBuilder = new EmbedBuilder
                {
                    Timestamp = DateTime.Now,
                    Color = Color.Red
                };
                await context.Channel.SendMessageAsync(embed: embedBuilder.WithDescription(result.ErrorReason).Build());
                var commandName = command.IsSpecified ? command.Value.Name : "An unknown command";


                if (result.Error == CommandError.BadArgCount ||
                    result.Error == CommandError.UnmetPrecondition)
                {
                    return;
                }
                _logger.Error($"MomentumCommandService {commandName} threw an error at {DateTime.Now}: {Environment.NewLine}{result.ErrorReason}");
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