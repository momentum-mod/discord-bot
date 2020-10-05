using DSharpPlus.CommandsNext.Attributes;

namespace MomentumDiscordBot.Commands.Checks
{
    public abstract class DescriptiveCheckBaseAttribute : CheckBaseAttribute
    {
        public string FailureResponse { get; set; }
    }
}