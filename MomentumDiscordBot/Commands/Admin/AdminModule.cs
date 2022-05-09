using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace MomentumDiscordBot.Commands.Admin
{
    public class AdminModule : AdminModuleBase
    {
        public DiscordClient DiscordClient { get; set; }

        [SlashCommand("forcereconnect", "Simulates the Discord API requesting a reconnect")]
        public async Task ForceReconnectAsync(InteractionContext context, [Option("seconds", "seconds")] long seconds)
        {
            await DiscordClient.DisconnectAsync();
            await Task.Delay((int)(seconds * 1000));
            await DiscordClient.ReconnectAsync();
        }
        
        [SlashCommand("forcerestart", "Forces the bot to exit the process, and have Docker auto-restart it")]
        public Task ForceRestartAsync(InteractionContext context)
        {
            Logger.Warning("{User} forced the bot to restart", context.User);
            
            // Safe exit
            Environment.Exit(0);

            return Task.CompletedTask;
        }
    }
}
