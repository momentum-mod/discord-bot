using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Commands.Moderator
{
    [SlashCommandGroup("trust", "media trust commands")]
    public class ModeratorMediaTrustModule : ModeratorModuleBase
    {
        public Configuration Config { get; set; }

        [SlashCommand("status", "Checks to see a member's media trust status")]
        public async Task MediaStatusAsync(InteractionContext context, [Option("member", "member")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Media Trust Status",
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = member.Username,
                    IconUrl = member.AvatarUrl ?? member.DefaultAvatarUrl
                }
            };

            await using var dbContext = DbContextHelper.GetNewDbContext(Config);

            var messages = await dbContext.DailyMessageCount.ToListAsync();
            var userMessages = messages.Where(x => x.UserId == member.Id).ToList();

            var oldestMessage = userMessages
                .OrderBy(x => x.Date)
                .FirstOrDefault();

            if (oldestMessage == null)
            {
                embedBuilder.WithColor(MomentumColor.Red)
                    .WithDescription("No recorded activity");
            }
            else
            {
                var totalMessageCount = userMessages.Sum(x => x.MessageCount);
                var oldestMessageSpan = DateTime.UtcNow - oldestMessage.Date;
                var hasTrustedRole = member.Roles.Any(x => x.Id == Config.MediaVerifiedRoleId);
                var hasBlacklistedRole = member.Roles.Any(x => x.Id == Config.MediaBlacklistedRoleId);


                embedBuilder.WithColor(MomentumColor.Blue)
                    .AddField("Oldest Message Sent", $"{oldestMessageSpan.ToPrettyFormat()} ago")
                    .AddField("Total Messages", totalMessageCount.ToString())
                    .AddField("Meets Requirements",
                        (oldestMessageSpan.TotalDays > Config.MediaMinimumDays &&
                        totalMessageCount > Config.MediaMinimumMessages).ToString())
                    .AddField("Has Trusted Role", hasTrustedRole.ToString())
                    .AddField("Has Blacklisted Role", hasBlacklistedRole.ToString());

                if (hasBlacklistedRole)
                {
                    embedBuilder.Color = MomentumColor.DarkestGray;
                }

                if (hasTrustedRole)
                {
                    embedBuilder.Color = MomentumColor.Green;
                }
            }

            await context.CreateResponseAsync(embed: embedBuilder.Build());
        }

        [SlashCommand("give", "Manually trusts a member, if applicable, removing the blacklist")]
        public async Task TrustUserAsync(InteractionContext context, [Option("member", "member")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            var trustedRole = context.Guild.GetRole(Config.MediaVerifiedRoleId);
            var blacklistRole = context.Guild.GetRole(Config.MediaBlacklistedRoleId);

            await member.GrantRoleAsync(trustedRole);
            await member.RevokeRoleAsync(blacklistRole);

            await ReplyNewEmbedAsync(context, "Trusted " + member.Mention, MomentumColor.Blue);
        }

        [SlashCommand("blacklist", "Manually blacklist a member, if applicable, removing the trust")]
        public async Task BlacklistUserAsync(InteractionContext context, [Option("member", "member")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            var trustedRole = context.Guild.GetRole(Config.MediaVerifiedRoleId);
            var blacklistRole = context.Guild.GetRole(Config.MediaBlacklistedRoleId);

            await member.RevokeRoleAsync(trustedRole);
            await member.GrantRoleAsync(blacklistRole);

            await ReplyNewEmbedAsync(context, "Blacklisted " + member.Mention, MomentumColor.Blue);
        }
    }
}
