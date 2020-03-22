using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Discord.Precondition
{
    public class ModeratorPreconditionAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.User is SocketGuildUser user)
            {
                var config = services.GetRequiredService<Config>();
                if (user.Roles.Any(x => x.Id == config.ModeratorRoleID))
                    return Task.FromResult(PreconditionResult.FromSuccess());
                return Task.FromResult(PreconditionResult.FromError("You must be a Moderator to use that command"));
            }

            return Task.FromResult(PreconditionResult.FromError("You must be a user"));
        }
    }
}