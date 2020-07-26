using MomentumDiscordBot.Discord.Precondition;

namespace MomentumDiscordBot.Discord.Commands.Admin
{
    [AdminPrecondition(Group = "Admin")]
    [DeveloperPrecondition(Group = "Admin")]
    [RequireAdminBotChannelPrecondition]
    public class AdminModuleBase : MomentumModuleBase { }
}