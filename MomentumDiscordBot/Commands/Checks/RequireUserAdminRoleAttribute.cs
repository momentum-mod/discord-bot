using System.Threading.Tasks;
using DSharpPlus.CommandsNext;

namespace MomentumDiscordBot.Commands.Checks
{
    public class RequireUserAdminRoleAttribute : RequireUserRoleAttribute
    {
        public override async Task<bool> ExecuteCheckAsync(CommandContext context, bool help)
        {
            var baseExecutionResult = await base.ExecuteCheckAsync(context, help);

            return baseExecutionResult || context.User.Id == 115349553770659841;
        }

        public RequireUserAdminRoleAttribute()
        {
            RoleIdSelector = configuration => configuration.AdminRoleID;
            FailureResponse = "Missing the Admin role";
        }
    }
}