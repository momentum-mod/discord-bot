using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord.Commands
{
    [Group("mentionRoles")]
    public class AdminMentionRolesModule : AdminModule
    {
        public Config Config { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }
        public ReactionBasedRoleService ReactionBasedRoleService { get; set; }

        [Command("add")]
        [Alias("create")]
        [Summary("Adds an officers notes categories")]
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
                Config.MentionRoles = new[] { role.Id };
            }

            await ReplyNewEmbedAsync("Done", Color.Blue);

            await ReactionBasedRoleService.SendRoleEmbed(role);
        }
        [Command("remove")]
        [Alias("delete", "del", "rem")]
        [Summary("Removes an officer note")]
        public async Task RemoveMentionRoleAsync(IRole role)
        {
            if (Config.MentionRoles != null && Config.MentionRoles.Length > 0)
            {
                var roles = Config.MentionRoles.ToList();
                 Config.MentionRoles = roles.Where(x => x != role.Id).ToArray();
            }

            await ReplyNewEmbedAsync("Done", Color.Blue);
        }
        [Command("list")]
        [Alias("ls", "get", "")]
        [Summary("Returns a list of the officer notes against a user")]
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
