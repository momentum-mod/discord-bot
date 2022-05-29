using MomentumDiscordBot.Commands.Checks;

namespace MomentumDiscordBot.Commands.Moderator
{
    [RequireUserModeratorRole]
    [RequireAdminBotChannel]
    [ContextMenuRequireUserModeratorRole]
    public class ModeratorModuleBase : MomentumModuleBase { }
}