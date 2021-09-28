using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Commands.Moderator
{
    public class ModeratorModule : ModeratorModuleBase
    {
        public StreamMonitorService StreamMonitorService { get; set; }
        
        public Configuration Config { get; set; }

        [Command("updatestreams")]
        [Description("Force an update of Twitch livestreams")]
        public async Task ForceUpdateStreamsAsync(CommandContext context)
        {
            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync(context, "Updating Livestreams", MomentumColor.Blue);
        }

        [Command("ban")]
        [Description("Bans a user, purging their messages")]
        public async Task BanAsync(CommandContext context, DiscordMember member)
        {
            await member.BanAsync(7, $"Banned by {context.User} using !ban");
            await ReplyNewEmbedAsync(context, $"Banned {member}, purging their messages in the last 7 days.",
                MomentumColor.Red);
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
    }
}