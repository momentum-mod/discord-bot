using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Discord.Precondition;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord.Commands
{
    [AdminPrecondition]
    [RequireAdminBotChannelPrecondition]
    public class AdminModule : MomentumModuleBase
    {
        public StreamMonitorService StreamMonitorService { get; set; }

        [Command("updatestreams")]
        [Summary("Force an update of Twitch livestreams")]
        public async Task ForceUpdateStreamsAsync()
        {
            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync("Updating Livestreams", Color.Blue);
        }

        [Command("membercount")]
        [Summary("Get the number of members with a role")]
        public async Task GetMembersWithRoleAsync(IRole role)
        {
            var guildRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == role.Id);

            if (guildRole != null)
            {
                var membersWithRole = Context.Guild.Users.Count(x => x.Roles.Contains(guildRole));
                await ReplyNewEmbedAsync($"{membersWithRole} users have {MentionUtils.MentionRole(guildRole.Id)}", Color.Blue);
            }
            else
            {
                await ReplyNewEmbedAsync("That role does not exist in this server", Color.Orange);
            }
        }
    }
}