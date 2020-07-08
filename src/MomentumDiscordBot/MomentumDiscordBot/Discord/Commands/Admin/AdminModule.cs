using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MomentumDiscordBot.Discord.Precondition;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord.Commands.Admin
{
    public class AdminModule : AdminModuleBase
    {
        public StreamMonitorService StreamMonitorService { get; set; }
        public DiscordSocketClient DiscordSocketClient { get; set; }       

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
        [Command("forcereconnect")]
        [Summary("Simulates the Discord API requesting a reconnect")]
        public async Task ForceReconnectAsync(int seconds)
        {
            await DiscordSocketClient.StopAsync();
            await Task.Delay(seconds * 1000);
            await DiscordSocketClient.StartAsync();
        }
    }
}