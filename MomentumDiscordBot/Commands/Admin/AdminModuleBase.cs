using MomentumDiscordBot.Commands.Checks;

namespace MomentumDiscordBot.Commands.Admin
{
    [RequireUserAdminRole]
    [RequireAdminBotChannel]
    public abstract class AdminModuleBase : MomentumModuleBase { }
}