using MomentumDiscordBot.Commands.Checks;

namespace MomentumDiscordBot.Commands.Moderator
{
    [RequireUserModeratorRole]
    [RequireAdminBotChannel]
    [ContextMenuRequireUserModeratorRole]
    // ContextMenus are used /info and /custom and *not* restricted to the bot channel
    // they are not actually part of the commandgroup and show up seperatly in the integration settings
    public class ModeratorModuleBase : MomentumModuleBase { }
}