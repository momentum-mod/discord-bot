using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Discord.Precondition
{
    public class AdminPreconditionAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.User is SocketGuildUser user)
            {
                var config = services.GetRequiredService<Config>();
                if (user.Roles.Any(x => x.Id == config.AdminRoleID))
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
                else
                {
                    return Task.FromResult(PreconditionResult.FromError($"You must be an Admin to use that command"));
                }
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("You must be a user"));
            }
        }
    }
}
