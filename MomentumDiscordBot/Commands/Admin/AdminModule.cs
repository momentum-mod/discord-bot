using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;

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
        
        public const string ForcerestartCommandName = "forcerestart";
        [SlashCommand(ForcerestartCommandName, "Forces the bot to exit the process, and have Docker auto-restart it")]
        public Task ForceRestartAsync(InteractionContext context)
        {
            Logger.Warning("{User} forced the bot to restart", context.User);
            
            _ = Task.Run(async () =>
            {
                await ReplyNewEmbedAsync(context, "Restarting ...", DiscordColor.Orange);
            });
            _ = Task.Run(async () =>
            {
                await Task.Delay(5 * 1000);
                // Safe exit
                Environment.Exit(0);
            });

            return Task.CompletedTask;
        }
    }
}
