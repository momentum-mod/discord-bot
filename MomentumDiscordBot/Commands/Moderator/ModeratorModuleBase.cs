using MomentumDiscordBot.Commands.Checks;

namespace MomentumDiscordBot.Commands.Moderator
{
    [RequireUserModeratorRole]
    [RequireAdminBotChannel]
    public class ModeratorModuleBase : MomentumModuleBase { }
}