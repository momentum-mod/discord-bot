using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;

namespace MomentumDiscordBot.Utilities
{
    public static class HelpCommandUtilities
    {
        public static Embed GetModuleHelpEmbed(ModuleInfo module, ICommandContext context, IServiceProvider services)
        {
            var title = $"Help: **({module.Name + (module.Aliases.Count > 1 ? ($"/{string.Join("/", module.Aliases.Skip(1))}") : "")})**";
            var validForCurrentUserCommands = module.Commands.Where(x => x.CheckPreconditionsAsync(context, services).GetAwaiter().GetResult().IsSuccess);

            var embedBuilder = new EmbedBuilder().WithTitle(title).WithColor(Color.Blue);
            foreach (var command in validForCurrentUserCommands)
            {
                embedBuilder.AddField($"**{'!' + (string.IsNullOrEmpty(module.Group) ? "" : module.Group + " ") + command.Name}** " + GetParametersString(command).TrimEnd(' ', ','), $"{(command.Summary == string.Empty ? "No description" : command.Summary)}. ");
            }

            return embedBuilder.Build();
        }

        private static string GetSummaryString(string summary) => string.IsNullOrEmpty(summary) ? "" : $"({summary})";
        private static string GetParametersString(CommandInfo command)
        {
            var output = $"{command.Parameters.Aggregate("", (currentString, nextParameter) => currentString + $"[{nextParameter.Name}{(GetSummaryString(nextParameter.Summary) == string.Empty ? "" : GetSummaryString(nextParameter.Summary))}] ")}";
            return output.Trim() == "Parameters:" ? "" : output;
        }
    }
}
