using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Commands.Checks
{
    public class RequireUserAdminRoleAttribute : RequireUserRoleAttribute
    {

        public override async Task<bool> ExecuteChecksAsync(InteractionContext context)
        {
            var baseExecutionResult = await base.ExecuteChecksAsync(context);
            if (baseExecutionResult)
            {
                return true;
            }

            var config = context.Services.GetRequiredService<Configuration>();
            
            return context.User.Id == config.DeveloperID;
        }

        public RequireUserAdminRoleAttribute()
        {
            RoleIdSelector = configuration => configuration.AdminRoleID;
            FailureResponse = "Missing the Admin role";
        }
    }
}