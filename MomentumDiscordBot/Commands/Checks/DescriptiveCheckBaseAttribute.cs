using DSharpPlus.SlashCommands;

namespace MomentumDiscordBot.Commands.Checks
{
    public abstract class DescriptiveCheckBaseAttribute : SlashCheckBaseAttribute
    {
        public string FailureResponse { get; set; }
    }
}