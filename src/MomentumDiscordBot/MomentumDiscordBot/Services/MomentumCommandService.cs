using Discord;
using Discord.Commands;

namespace MomentumDiscordBot.Services
{
    public class MomentumCommandService
    {
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
