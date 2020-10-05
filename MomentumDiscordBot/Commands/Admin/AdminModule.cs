using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Commands.Admin
{
    public class AdminModule : AdminModuleBase
    {
        public StreamMonitorService StreamMonitorService { get; set; }
        public DiscordClient DiscordSocketClient { get; set; }

        [Command("updatestreams")]
        [Description("Force an update of Twitch livestreams")]
        public async Task ForceUpdateStreamsAsync(CommandContext context)
        {
            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync(context, "Updating Livestreams", MomentumColor.Blue);
        }

        [Command("membercount")]
        [Description("Get the number of members with a role")]
        public async Task GetMembersWithRoleAsync(CommandContext context, DiscordRole role)
        {
            var (_, guildRole) = context.Guild.Roles.FirstOrDefault(x => x.Key == role.Id);

            if (guildRole != null)
            {
                var membersWithRole = context.Guild.Members.Values.Count(x => x.Roles.Contains(guildRole));
                await ReplyNewEmbedAsync(context, $"{membersWithRole} users have {guildRole.Mention}",
                    MomentumColor.Blue);
            }
            else
            {
                await ReplyNewEmbedAsync(context, "That role does not exist in this server", DiscordColor.Orange);
            }
        }

        [Command("forcereconnect")]
        [Description("Simulates the Discord API requesting a reconnect")]
        public async Task ForceReconnectAsync(CommandContext context, int seconds)
        {
            await DiscordSocketClient.DisconnectAsync();
            await Task.Delay(seconds * 1000);
            await DiscordSocketClient.ReconnectAsync();
        }
    }
}