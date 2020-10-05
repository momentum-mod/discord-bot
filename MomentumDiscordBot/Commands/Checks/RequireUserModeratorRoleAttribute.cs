namespace MomentumDiscordBot.Commands.Checks
{
    public class RequireUserModeratorRoleAttribute : RequireUserRoleAttribute
    {
        public RequireUserModeratorRoleAttribute()
        {
            RoleIdSelector = configuration => configuration.ModeratorRoleID;
            FailureResponse = "Missing the Moderator role";
        }
    }
}