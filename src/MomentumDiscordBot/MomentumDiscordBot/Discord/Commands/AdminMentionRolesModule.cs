using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord.Commands
{
    [Group("mentionRoles")]
    public class AdminMentionRolesModule : AdminModule
    {
        public Config Config { get; set; }
        public ReactionBasedRoleService ReactionBasedRoleService { get; set; }

        [Command("add")]
        [Alias("create")]
        public async Task CreateMentionRoleAsync(IRole role)
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
                    await ReplyNewEmbedAsync("That role is already added.", Color.Orange);
                    return;
                }
            }
            else
            {
                Config.MentionRoles = new[] {role.Id};
            }

            await ReplyNewEmbedAsync("Done", Color.Blue);

            await ReactionBasedRoleService.SendRoleEmbed(role);
        }

        [Command("remove")]
        [Alias("delete", "del", "rem")]
        public async Task RemoveMentionRoleAsync(IRole role)
        {
            if (Config.MentionRoles != null && Config.MentionRoles.Length > 0)
            {
                Config.MentionRoles = Config.MentionRoles.Where(x => x != role.Id).ToArray();
            }

            await ReactionBasedRoleService.RemoveRoleEmbed(role);

            await ReplyNewEmbedAsync("Done", Color.Blue);
        }

        [Command("list")]
        [Alias("ls", "get", "")]
        public async Task ListMentionRolesAsync()
        {
            var mentionRoles = Context.Guild.Roles.Where(x => Config.MentionRoles.Contains(x.Id));
            var embed = new EmbedBuilder
            {
                Title = "**Notification Roles**",
                Description = string.Join(Environment.NewLine, mentionRoles),
                Color = Color.Blue
            }.Build();

            await ReplyAsync(embed: embed);
        }
    }
}