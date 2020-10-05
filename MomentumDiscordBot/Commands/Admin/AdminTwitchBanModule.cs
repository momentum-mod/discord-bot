using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Commands.Admin
{
    [Group("twitchBan")]
    public class AdminTwitchBanModule : AdminModuleBase
    {
        public Configuration Config { get; set; }
        public StreamMonitorService StreamMonitorService { get; set; }


        [Command("add")]
        [Aliases("create")]
        [Description("Hard ban a twitch user from the livestream channel")]
        public async Task AddTwitchBanAsync(CommandContext context, [RemainingText] string username)
        {
            var bans = (Config.TwitchUserBans ?? new string[0]).ToList();

            var userToBanId = await StreamMonitorService.TwitchApiService.GetOrDownloadTwitchIDAsync(username);

            if (userToBanId == null)
            {
                await ReplyNewEmbedAsync(context, "Error getting the user's ID from the Twitch API, please try again.",
                    DiscordColor.Orange);
                return;
            }

            bans.Add(userToBanId);
            Config.TwitchUserBans = bans.ToArray();

            await Config.SaveToFileAsync();

            // Force update
            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync(context, $"Banned user with ID: {userToBanId}", DiscordColor.Orange);
        }

        [Command("remove")]
        [Aliases("delete", "del", "rem")]
        [Description("Hard unban a twitch user from the livestream channel")]
        public async Task RemoveTwitchBanAsync(CommandContext context, [RemainingText] string username)
        {
            var bans = (Config.TwitchUserBans ?? new string[0]).ToList();

            var userToUnbanId = await StreamMonitorService.TwitchApiService.GetOrDownloadTwitchIDAsync(username);

            if (userToUnbanId == null)
            {
                await ReplyNewEmbedAsync(context, "Error getting the user's ID from the Twitch API, please try again.",
                    DiscordColor.Orange);
                return;
            }

            bans.Remove(userToUnbanId);
            Config.TwitchUserBans = bans.ToArray();

            await Config.SaveToFileAsync();

            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync(context, $"Unbanned user with ID: {userToUnbanId}", DiscordColor.Orange);
        }

        [Command("list")]
        [Aliases("ls", "get")]
        [Description("Get a list of Twitch users hard banned from the livestream channel")]
        public async Task ListTwitchBanAsync(CommandContext context)
        {
            var bans = Config.TwitchUserBans ?? new string[0];

            var banUsernameTasks =
                bans.Select(async x => await StreamMonitorService.TwitchApiService.GetStreamerNameAsync(x));
            var usernames = await Task.WhenAll(banUsernameTasks);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Twitch Banned IDs",
                Description = Formatter.Sanitize(string.Join(Environment.NewLine, usernames)),
                Color = MomentumColor.Blue
            }.Build();

            await context.RespondAsync(embed: embed);
        }
    }
}