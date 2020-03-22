using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MomentumDiscordBot.Utilities
{
    public static class PreconditionUtility
    {
        public static Task<PreconditionResult> RequireRoleAsync(this IUser user, ulong roleId)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            if (!(user is SocketGuildUser guildUser))
                return Task.FromResult(PreconditionResult.FromError("You must be a guild user"));

            return Task.FromResult(guildUser.Roles.Any(x => x.Id == roleId)
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("You must be an Admin to use that command"));
        }
    }
}