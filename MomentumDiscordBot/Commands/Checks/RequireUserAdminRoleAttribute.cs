namespace MomentumDiscordBot.Commands.Checks
{
    public class RequireUserAdminRoleAttribute : RequireUserRoleAttribute
    {
        public RequireUserAdminRoleAttribute()
        {
            RoleIdSelector = configuration => configuration.AdminRoleID;
            FailureResponse = "Missing the Admin role";
        }
    }
}