using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Discord.Commands
{
    [Group("twitchBan")]
    public class AdminTwitchBanModule : AdminModule
    {
        public Config Config { get; set; }
        public StreamMonitorService StreamMonitorService { get; set; }

        [Command("add")]
        [Alias("create")]
        [Summary("Hard ban a twitch user from the livestream channel")]
        public async Task AddTwitchBanAsync([Remainder] string username)
        {
            var bans = (Config.TwitchUserBans ?? new string[0]).ToList();

            var userToBanId = await StreamMonitorService.GetTwitchIDAsync(username);

            bans.Add(userToBanId);
            Config.TwitchUserBans = bans.ToArray();

            // Force update
            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync($"Banned user with ID: {userToBanId}", Color.Orange);
        }

        [Command("remove")]
        [Alias("delete", "del", "rem")]
        [Summary("Hard unban a twitch user from the livestream channel")]
        public async Task RemoveTwitchBanAsync([Remainder] string username)
        {
            var bans = (Config.TwitchUserBans ?? new string[0]).ToList();

            var userToUnbanId = await StreamMonitorService.GetTwitchIDAsync(username);

            bans.Remove(userToUnbanId);
            Config.TwitchUserBans = bans.ToArray();

            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync($"Unbanned user with ID: {userToUnbanId}", Color.Orange);
        }

        [Command("list")]
        [Alias("ls", "get")]
        [Summary("Get a list of Twitch users hard banned from the livestream channel")]
        public async Task ListTwitchBanAsync()
        {
            var bans = Config.TwitchUserBans ?? new string[0];

            var banUsernameTasks = bans.Select(async x => await StreamMonitorService.TwitchApiService.GetStreamerNameAsync(x));
            var usernames = await Task.WhenAll(banUsernameTasks);

            var embed = new EmbedBuilder
            {
                Title = "Twitch Banned IDs",
                Description = string.Join(Environment.NewLine, usernames).EscapeDiscordChars(),
                Color = Color.Blue
            }.Build();

            await ReplyAsync(embed: embed);
        }
    }
}
