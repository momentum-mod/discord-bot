using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Commands.Admin
{
    [Group("mentionRoles")]
    public class AdminMentionRolesModule : AdminModuleBase
    {
        public Configuration Config { get; set; }
        public ReactionBasedRoleService ReactionBasedRoleService { get; set; }

        [Command("add")]
        [Aliases("create")]
        [Description("Create a role for mentioning, and add it to the react channel")]
        public async Task CreateMentionRoleAsync(CommandContext context, DiscordRole role)
        {
            if (Config.MentionRoles != null)
            {
                if (!Config.MentionRoles.Contains(role.Id))
                {
                    var mentionRoles = new ulong[Config.MentionRoles.Length + 1];
                    Config.MentionRoles.CopyTo(mentionRoles, 0);
                    mentionRoles[^1] = role.Id;

                    Config.MentionRoles = mentionRoles;
                }
                else
                {
                    await ReplyNewEmbedAsync(context, "That role is already added.", DiscordColor.Orange);
                    return;
                }
            }
            else
            {
                Config.MentionRoles = new[] {role.Id};
            }

            await ReplyNewEmbedAsync(context, "Done", MomentumColor.Blue);

            await ReactionBasedRoleService.SendRoleEmbed(role);
        }

        [Command("remove")]
        [Aliases("delete", "del", "rem")]
        [Description("Removes a role for mentioning, and removes it from the react channel")]
        public async Task RemoveMentionRoleAsync(CommandContext context, DiscordRole role)
        {
            if (Config.MentionRoles != null && Config.MentionRoles.Length > 0)
            {
                Config.MentionRoles = Config.MentionRoles.Where(x => x != role.Id).ToArray();
            }

            await ReactionBasedRoleService.RemoveRoleEmbed(role);

            await ReplyNewEmbedAsync(context, "Done", MomentumColor.Blue);
        }

        [Command("list")]
        [Aliases("ls", "get")]
        [Description("Get a list of the mention roles")]
        public async Task ListMentionRolesAsync(CommandContext context)
        {
            var mentionRoles = context.Guild.Roles.Where(x => Config.MentionRoles.Contains(x.Key));
            var embed = new DiscordEmbedBuilder
            {
                Title = "**Notification Roles**",
                Description = string.Join(Environment.NewLine, mentionRoles),
                Color = MomentumColor.Blue
            }.Build();

            await context.RespondAsync(embed: embed);
        }
    }
}