using MomentumDiscordBot.Discord.Precondition;

namespace MomentumDiscordBot.Discord.Commands.Moderator
{
    [ModeratorPrecondition(Group = "Moderator")]
    [RequireAdminBotChannelPrecondition]
    public class ModeratorModuleBase : MomentumModuleBase { }
}
