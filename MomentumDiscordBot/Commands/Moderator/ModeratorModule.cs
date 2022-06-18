using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;
using MomentumDiscordBot.Commands.Autocomplete;

namespace MomentumDiscordBot.Commands.Moderator
{
    public class ModeratorModule : ModeratorModuleBase
    {
        public StreamMonitorService StreamMonitorService { get; set; }

        public Configuration Config { get; set; }

        [SlashCommand("updatestreams", "Force an update of Twitch livestreams")]
        public async Task ForceUpdateStreamsAsync(InteractionContext context)
        {
            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync(context, "Updating Livestreams", MomentumColor.Blue);
        }

        [SlashCommand("ban", "Bans a user, purging their messages")]
        public async Task BanAsync(InteractionContext context, [Option("member", "member")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            await member.BanAsync(7, $"Banned by {context.User} using !ban");
            await ReplyNewEmbedAsync(context, $"Banned {member}, purging their messages in the last 7 days.",
                MomentumColor.Red);
        }

        [SlashCommand("membercount", "Get the number of members with a role")]
        public async Task GetMembersWithRoleAsync(InteractionContext context, [Option("role", "role")] DiscordRole role)
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

        [SlashCommand("status", "Sets the bots status")]
        public async Task StatusAsync(InteractionContext context,
            [Option("status", "status")] string status,
            [ChoiceProvider(typeof(ActivityTypeChoiceProvider))][Option("type", "ActivityType")] string type = null)
        {
            var activity = Enum.TryParse(type, out ActivityType activityType)
                ? new DiscordActivity(status, activityType)
                : new DiscordActivity(status);
            await context.Client.UpdateStatusAsync(activity);
            await ReplyNewEmbedAsync(context, $"Status set to '{status}'.", MomentumColor.Blue);

        }
    }
}
