using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord.Commands
{
    [Group("twitchban")]
    public class AdminTwitchBanModule : AdminModule
    {
        public Config Config { get; set; }
        public StreamMonitorService StreamMonitorService { get; set; }

        [Command("add")]
        [Alias("create")]
        public async Task AddTwitchBanAsync([Remainder] string username)
        {
            var bans = (Config.TwitchUserBans ?? new string[0]).ToList();

            var userToBanId = await StreamMonitorService.GetTwitchIDAsync(username);

            bans.Add(userToBanId);
            Config.TwitchUserBans = bans.ToArray();

            await ReplyNewEmbedAsync($"Banned user with ID: {userToBanId}", Color.Orange);
        }

        [Command("remove")]
        [Alias("delete", "del", "rem")]
        public async Task RemoveTwitchBanAsync([Remainder] string username)
        {
            var bans = (Config.TwitchUserBans ?? new string[0]).ToList();

            var userToUnbanId = await StreamMonitorService.GetTwitchIDAsync(username);

            bans.Remove(userToUnbanId);
            Config.TwitchUserBans = bans.ToArray();

            await ReplyNewEmbedAsync($"Unbanned user with ID: {userToUnbanId}", Color.Orange);
        }

        [Command("list")]
        [Alias("ls", "get")]
        public async Task ListTwitchBanAsync()
        {
            var bans = Config.TwitchUserBans ?? new string[0];

            var embed = new EmbedBuilder
            {
                Title = "Twitch Banned IDs",
                Description = string.Join(Environment.NewLine, bans),
                Color = Color.Blue
            }.Build();

            await ReplyAsync(embed: embed);
        }
    }
}
