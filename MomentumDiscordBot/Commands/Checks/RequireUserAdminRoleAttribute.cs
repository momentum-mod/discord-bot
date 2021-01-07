using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Commands.Checks
{
    public class RequireUserAdminRoleAttribute : RequireUserRoleAttribute
    {
        public override async Task<bool> ExecuteCheckAsync(CommandContext context, bool help)
        {
            var baseExecutionResult = await base.ExecuteCheckAsync(context, help);
            if (baseExecutionResult)
            {
                return true;
            }

            var config = context.CommandsNext.Services.GetRequiredService<Configuration>();
            
            return context.User.Id == config.DeveloperID;
        }

        public RequireUserAdminRoleAttribute()
        {
            RoleIdSelector = configuration => configuration.AdminRoleID;
            FailureResponse = "Missing the Admin role";
        }
    }
}