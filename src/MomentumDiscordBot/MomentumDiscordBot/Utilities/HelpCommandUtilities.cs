using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Discord.Commands;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Utilities
{
    public static class HelpCommandUtilities
    {
        public static async Task<Embed> GetModuleHelpEmbed(ModuleInfo module, ICommandContext context, IServiceProvider services,
            Config config) =>
            (await new EmbedBuilder()
                .BuildTitle(module)
                .AddValidCommandFields(module, context, services, config))
            .WithColor(MomentumColor.Blue)
            .Build();

        private static async Task<EmbedBuilder> AddValidCommandFields(this EmbedBuilder embedBuilder, ModuleInfo module,
            ICommandContext context, IServiceProvider services, Config config)
            => (await GetValidCommandsAsync(module, context, services))
                .Aggregate(embedBuilder, (currentEmbedBuilder, nextCommand)
                    => currentEmbedBuilder.AddCommandField(nextCommand, module, config));

        private static EmbedBuilder AddCommandField(this EmbedBuilder embedBuilder, CommandInfo command,
            ModuleInfo module, Config config)
        {
            var commandString = config.CommandPrefix;
            if (!string.IsNullOrEmpty(module.Group))
            {
                // Add the command group prefix
                commandString += module.Group + " ";
            }

            commandString += command.Name;

            return embedBuilder.AddField($"**{commandString}** " + GetParametersString(command),
                GetCommandSummary(command));
        }

        private static async Task<IEnumerable<CommandInfo>> GetValidCommandsAsync(ModuleInfo module, ICommandContext context,
            IServiceProvider services)
        {
            var preconditionCheckTasks = module.Commands.Select(async command =>
            {
                var valid = command.Attributes.All(y => y.GetType() != typeof(HiddenAttribute)) &&
                    (await command.CheckPreconditionsAsync(context, services)).IsSuccess;

                return (Command: command, HasPermission:valid);
            });

            return (await Task.WhenAll(preconditionCheckTasks))
                .Where(x => x.HasPermission)
                .Select(x => x.Command);
        }

        private static EmbedBuilder BuildTitle(this EmbedBuilder embedBuilder, ModuleInfo module)
        {
            var moduleNamesString = module.Name;
            if (module.Aliases.Count > 1)
            {
                // Skip 1 alias, as the first 'alias' is the full name
                moduleNamesString += $"/{string.Join("/", module.Aliases.Skip(1))}";
            }

            var title = $"Help: **({moduleNamesString})**";

            return embedBuilder.WithTitle(title);
        }

        private static string GetSummaryString(string summary) => string.IsNullOrEmpty(summary) ? "" : $"({summary})";

        private static string GetParametersString(CommandInfo command)
            => command.Parameters.Aggregate(string.Empty, (current, parameter)
                => current + $"[{parameter.Name}{GetSummaryString(parameter.Summary)}] ");

        private static string GetCommandSummary(CommandInfo command)
        {
            if (string.IsNullOrEmpty(command.Summary))
            {
                return "No description provided";
            }

            if (command.Summary.EndsWith('.'))
            {
                return command.Summary;
            }

            return command.Summary + '.';
        }
    }
}