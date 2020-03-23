using MomentumDiscordBot.Discord.Precondition;

namespace MomentumDiscordBot.Discord.Commands
{
    [AdminPrecondition]
    [RequireAdminBotChannelPrecondition]
    public class AdminModule : MomentumModuleBase { }
}