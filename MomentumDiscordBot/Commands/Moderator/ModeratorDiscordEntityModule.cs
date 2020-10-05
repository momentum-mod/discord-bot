using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Commands.Moderator
{
    [Group("discord")]
    public class ModeratorDiscordEntityModule : ModeratorModuleBase
    {
        [Command("info")]
        [Models.Hidden]
        [Priority(-1000)]
        public async Task EntityNotFoundOverwriteAsync(CommandContext context, [RemainingText] string param)
        {
            await ReplyNewEmbedAsync(context, $"No discord entity found for {param}", DiscordColor.Orange);
        }

        [Command("info")]
        [Description("Provides information about a user")]
        public async Task GetUserInfoAsync(CommandContext context, DiscordMember member)
        {
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

            await context.RespondAsync(embed: embed.Build());
        }

        [Command("info")]
        [Description("Provides information about a channel")]
        public async Task GetChannelInfoAsync(CommandContext context, DiscordChannel channel)
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

            await context.RespondAsync(embed: embed.Build());
        }

        [Command("info")]
        [Description("Provides information about a role")]
        public async Task GetRoleInfoAsync(CommandContext context, DiscordRole role)
        {
            var embed = new DiscordEmbedBuilder
            {
                Description = role.Mention,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = role.Id.ToString()
                }
            };

            await context.RespondAsync(embed: embed.Build());
        }

        [Command("info")]
        [Description("Provides information about a message")]
        [Priority(5000)]
        public async Task GetMessageInfoAsync(CommandContext context, DiscordMessage message)
        {
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

            await context.RespondAsync(embed: embed.Build());
        }
    }
}