using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Services
{
    [Microservice(MicroserviceType.InjectAndInitialize)]
    public class SlashCommandService
    {
        public SlashCommandService(Configuration config, DiscordClient discordClient, IServiceProvider services)
        {
            var commands = discordClient.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            });

            commands.RegisterCommands(Assembly.GetEntryAssembly());

            commands.SlashCommandErrored += _commands_SlashCommandErrored;
        }

        private Task _commands_SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Exception is SlashExecutionChecksFailedException exception)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Access Denied",
                        Description = exception.FailedChecks.ToCleanResponse(),
                        Color = MomentumColor.Red
                    };

                    await e.Context.CreateResponseAsync(embed: embed);
                }
                else
                {
                    e.Context.Client.Logger.LogError(
                        $"{e.Context.User.Username} tried executing '{e.Context.CommandName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}",
                        DateTime.Now);
                }
            });

            return Task.CompletedTask;
        }
    }
}
