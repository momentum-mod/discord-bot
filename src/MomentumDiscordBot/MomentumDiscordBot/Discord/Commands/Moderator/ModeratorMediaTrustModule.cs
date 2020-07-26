using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Discord.Commands.Moderator
{
    [Group("trust")]
    public class ModeratorMediaTrustModule : ModeratorModuleBase
    {
        public Config Config { get; set; }

        [Command("status")]
        [Summary("Checks to see a user's media trust status")]
        public async Task MediaStatusAsync(IGuildUser user)
        {
            var embedBuilder = new EmbedBuilder
            {
                Title = "Media Trust Status",
                Author = new EmbedAuthorBuilder()
                    .WithName(user.Username)
                    .WithIconUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
            };

            await using var dbContext = DbContextHelper.GetNewDbContext(Config);

            var messages = await dbContext.DailyMessageCount.ToListAsync();
            var userMessages = messages.Where(x => x.UserId == user.Id).ToList();

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
                var hasTrustedRole = user.RoleIds.Any(x => x == Config.MediaVerifiedRoleId);
                var hasBlacklistedRole = user.RoleIds.Any(x => x == Config.MediaBlacklistedRoleId);


                embedBuilder.WithColor(MomentumColor.Blue)
                    .AddField("Oldest Message Sent", $"{oldestMessageSpan.ToPrettyFormat()} ago")
                    .AddField("Total Messages", totalMessageCount)
                    .AddField("Meets Requirements",
                        oldestMessageSpan.TotalDays > Config.MediaMinimumDays &&
                        totalMessageCount > Config.MediaMinimumMessages)
                    .AddField("Has Trusted Role", hasTrustedRole)
                    .AddField("Has Blacklisted Role", hasBlacklistedRole);

                if (hasBlacklistedRole)
                {
                    embedBuilder.Color = MomentumColor.DarkestGray;
                }

                if (hasTrustedRole)
                {
                    embedBuilder.Color = MomentumColor.Green;
                }
            }

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("")]
        [Summary("Manually trusts a user, if applicable, removing the blacklist")]
        public async Task TrustUserAsync(IGuildUser user)
        {
            var trustedRole = Context.Guild.GetRole(Config.MediaVerifiedRoleId);
            var blacklistRole = Context.Guild.GetRole(Config.MediaBlacklistedRoleId);

            await user.AddRoleAsync(trustedRole);
            await user.RemoveRoleAsync(blacklistRole);

            await ReplyNewEmbedAsync("Trusted " + MentionUtils.MentionUser(user.Id), MomentumColor.Blue);
        }

        [Command("blacklist")]
        [Summary("Manually blacklist a user, if applicable, removing the trust")]

        public async Task BlacklistUserAsync(IGuildUser user)
        {
            var trustedRole = Context.Guild.GetRole(Config.MediaVerifiedRoleId);
            var blacklistRole = Context.Guild.GetRole(Config.MediaBlacklistedRoleId);

            await user.RemoveRoleAsync(trustedRole);
            await user.AddRoleAsync(blacklistRole);

            await ReplyNewEmbedAsync("Blacklisted " + MentionUtils.MentionUser(user.Id), MomentumColor.Blue);
        }
    }
}