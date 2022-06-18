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

        [SlashCommand("channel", "Provides information about a channel")]
        public static async Task GetChannelInfoAsync(InteractionContext context, [Option("channel", "channel")] DiscordChannel channel)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = channel.Type switch
                {
                    ChannelType.Category => $"Category: {channel.Name}",
                    ChannelType.Text => $"# {channel.Name}",
                    ChannelType.Voice => $"🔊 {channel.Name}",
                    _ => channel.Name
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = channel.Id.ToString()
                }
            };

            await context.CreateResponseAsync(embed: embed);
        }

        [SlashCommand("role", "Provides information about a role")]
        public static async Task GetRoleInfoAsync(InteractionContext context, [Option("role", "role")] DiscordRole role)
        {
            var embed = new DiscordEmbedBuilder
            {
                Description = role.Mention,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = role.Id.ToString()
                }
            };

            await context.CreateResponseAsync(embed: embed);
        }

        [ContextMenu(ApplicationCommandType.MessageContextMenu, "message info")]
        public static async Task GetMessageInfoAsync(ContextMenuContext context)
        {
            DiscordMessage message = context.TargetMessage;
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = message.Author.Username,
                    IconUrl = message.Author.AvatarUrl ?? message.Author.DefaultAvatarUrl
                },
                Description = "Message: " + string.Join("", message.Content.Take(1024)),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = message.Id.ToString()
                }
            };

            embed.AddField("Jump", message.JumpLink.ToString());

            await context.CreateResponseAsync(embed: embed);
        }
    }
}
