using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using MomentumDiscordBot.Constants;

namespace MomentumDiscordBot.Commands
{
    public class HelpFormatter : DefaultHelpFormatter
    {
        public HelpFormatter(CommandContext context) : base(context)
        {
            // Everything is fine current to be default, just nice to use momentum colors
            EmbedBuilder.WithColor(MomentumColor.Blue);
        }
    }
}