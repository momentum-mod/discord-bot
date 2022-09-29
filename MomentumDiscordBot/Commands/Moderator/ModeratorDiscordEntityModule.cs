using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Commands.Moderator
{
    [SlashCommandGroup("info", "Provides information about ...")]
    public class ModeratorDiscordEntityModule : ModeratorModuleBase
    {
        [SlashCommand("user", "Provides information about a user")]
        public static async Task GetUserInfoAsync(InteractionContext context, [Option("member", "member")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            var avatarUrl = member.AvatarUrl ?? member.DefaultAvatarUrl;
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = avatarUrl,
                    Name = member.Username
                }
            };

            // If the user doesn't have a role then default to blue
            var highestRole = member.Roles.OrderByDescending(x => x.Position).FirstOrDefault();
            embed.Color = highestRole?.Color ?? MomentumColor.Blue;

            embed.AddField("Mention", member.Mention);

            if (member.Roles.Any())
            {
                embed.AddField("Roles",
                    string.Join(" ",
                        member.Roles.OrderByDescending(x => x.Position).Select(x => x.Mention)));
            }

            var dangerousPermissions = member.Roles.Select(x => x.Permissions)
                .Aggregate(Permissions.None, (nextPermission, allPermissions) => allPermissions | nextPermission)
                .GetDangerousPermissions().ToPermissionString();
            if (dangerousPermissions.Any())
            {
                embed.AddField("Dangerous Permissions",
                    string.Join(" ", dangerousPermissions));
            }

            embed.AddField("Joined",
                $"{(DateTime.UtcNow - member.JoinedAt).ToPrettyFormat()} ago");


            embed.AddField("Account Created",
                $"{(DateTime.UtcNow - member.CreationTimestamp).ToPrettyFormat()} ago");

            embed.WithFooter(member.Id.ToString());

            await context.CreateResponseAsync(embed: embed);
        }

        [SlashCommand("role", "Provides information about a role")]
        public static async Task GetRoleInfoAsync(InteractionContext context, [Option("role", "role")] DiscordRole role)
        {
            string membersWithRoleMsg;
            if (role.Name == "@everyone")
            {
                membersWithRoleMsg = $"There are {context.Guild.Members.Count} members in total.\n";
            }
            else
            {
                var membersWithRole = context.Guild.Members.Values.Count(x => x.Roles.Contains(role));
                membersWithRoleMsg = $"{membersWithRole} users have {role.Mention}.\n";
            }
            var embed = new DiscordEmbedBuilder
            {
                Description = membersWithRoleMsg +
                              "See 'Members' page in the server settings for details"
            };

            await context.CreateResponseAsync(embed: embed);
        }
    }
}
