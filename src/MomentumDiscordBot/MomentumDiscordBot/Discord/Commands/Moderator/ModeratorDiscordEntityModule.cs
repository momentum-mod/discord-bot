using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Discord.Precondition;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Discord.Commands.Moderator
{
    [DeveloperPrecondition(Group = "Moderator")]
    [Group("discord")]
    public class ModeratorDiscordEntityModule : ModeratorModuleBase
    {
        [Command("info")]
        [Hidden]
        [Priority(-1000)]
        public async Task EntityNotFoundOverwriteAsync([Remainder] string param)
        {
            await ReplyNewEmbedAsync($"No discord entity found for {param}", Color.Orange);
        }

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
                embed.Color = highestRole?.Color ?? MomentumColor.Blue;

                embed.AddField("Mention", socketGuildUser.Mention);

                if (socketGuildUser.Roles.Any())
                {
                    embed.AddField("Roles",
                        string.Join(" ",
                            socketGuildUser.Roles.OrderByDescending(x => x.Position).Select(x => x.Mention)));
                }

                var dangerousPermissions = socketGuildUser.GuildPermissions.GetDangerousPermissions().ToList();
                if (dangerousPermissions.Any())
                {
                    embed.AddField("Dangerous Permissions",
                        string.Join(" ", dangerousPermissions));
                }

                if (socketGuildUser.JoinedAt.HasValue)
                {
                    embed.AddField("Joined",
                        $"{(DateTime.UtcNow - socketGuildUser.JoinedAt.Value).ToPrettyFormat()} ago");
                }

                embed.AddField("Account Created",
                    $"{(DateTime.UtcNow - socketGuildUser.CreatedAt).ToPrettyFormat()} ago");

                embed.WithFooter(socketGuildUser.Id.ToString());

                await ReplyAsync(embed: embed.Build());
            }
        }

        [Command("info")]
        [Summary("Provides information about a channel")]
        public async Task GetChannelInfoAsync([Remainder] IChannel channel)
        {
            var embed = new EmbedBuilder
            {
                Title = channel switch
                {
                    ICategoryChannel categoryChannel => $"Category: {categoryChannel.Name}",
                    ITextChannel textChannel => $"# {textChannel.Name}",
                    IVoiceChannel voiceChannel => $"🔊 {voiceChannel.Name}",
                    _ => channel.Name
                },
                Footer = new EmbedFooterBuilder().WithText(channel.Id.ToString())
            };

            await ReplyAsync(embed: embed.Build());
        }

        [Command("info")]
        [Summary("Provides information about a role")]
        public async Task GetRoleInfoAsync([Remainder] IRole role)
        {
            var embed = new EmbedBuilder
            {
                Description = role.Mention,
                Footer = new EmbedFooterBuilder().WithText(role.Id.ToString())
            };

            await ReplyAsync(embed: embed.Build());
        }

        [Command("info")]
        [Summary("Provides information about a message")]
        [Priority(5000)]
        public async Task GetMessageInfoAsync([Remainder] IMessage message)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = message.Author.Username,
                    IconUrl = message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl()
                },
                Description = "Message: " + string.Join("", message.Content.Take(1024)),
                Footer = new EmbedFooterBuilder().WithText(message.Id.ToString())
            };

            embed.AddField("Jump", message.GetJumpUrl());

            await ReplyAsync(embed: embed.Build());
        }
    }
}