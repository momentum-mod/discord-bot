using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Discord.Commands.Moderator
{
    [Group("discord")]
    public class ModeratorDiscordEntityModule : ModeratorModule
    {
        [Command("info")]
        [Summary("Provides information about a user")]
        public async Task GetUserInfoAsync([Remainder] IUser user)
        {
            // We can cast without caring because RequireModerator requires a guild user
            if (user is SocketGuildUser socketGuildUser)
            {
                var avatarUrl = socketGuildUser.GetAvatarUrl() ?? socketGuildUser.GetDefaultAvatarUrl();
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder().WithIconUrl(avatarUrl).WithName(socketGuildUser.Username)
                };

                // If the user doesn't have a role then default to blue
                var highestRole = socketGuildUser.Roles.OrderByDescending(x => x.Position).FirstOrDefault();
                embed.Color = highestRole?.Color ?? Color.Blue;

                embed.AddField("Mention", socketGuildUser.Mention);

                if (socketGuildUser.Roles.Any())
                {
                    embed.AddField("Roles",
                        string.Join(" ", socketGuildUser.Roles.OrderByDescending(x => x.Position).Select(x => x.Mention)));
                }

                var dangerousPermissions = socketGuildUser.GuildPermissions.GetDangerousPermissions().ToList();
                if (dangerousPermissions.Any())
                {
                    embed.AddField("Dangerous Permissions",
                        string.Join(" ", dangerousPermissions));
                }

                if (socketGuildUser.JoinedAt.HasValue)
                {
                    embed.AddField("Joined", $"{(DateTime.UtcNow - socketGuildUser.JoinedAt.Value).ToPrettyFormat()} ago");
                }

                embed.AddField("Account Created", $"{(DateTime.UtcNow - socketGuildUser.CreatedAt).ToPrettyFormat()} ago");

                embed.WithFooter(socketGuildUser.Id.ToString());

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}
