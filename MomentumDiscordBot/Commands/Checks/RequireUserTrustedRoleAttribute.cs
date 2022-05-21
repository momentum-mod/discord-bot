namespace MomentumDiscordBot.Commands.Checks
{
    public class RequireUserTrustedRoleAttribute : RequireUserRoleAttribute
    {
        public RequireUserTrustedRoleAttribute()
        {
            RoleIdSelector = configuration => configuration.MediaVerifiedRoleId;
            FailureResponse = "Missing the Trusted role";
        }
    }
}