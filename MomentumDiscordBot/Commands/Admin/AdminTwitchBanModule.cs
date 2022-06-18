using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Commands.Admin
{
    [SlashCommandGroup("twitchBan", "twitch ban commands")]
    public class AdminTwitchBanModule : AdminModuleBase
    {
        public Configuration Config { get; set; }
        public StreamMonitorService StreamMonitorService { get; set; }


        [SlashCommand("add", "Hard ban a twitch user from the livestream channel")]
        public async Task AddTwitchBanAsync(InteractionContext context, [Option("RemainingText", "RemainingText")] string username)
        {
            var bans = (Config.TwitchUserBans ?? Array.Empty<string>()).ToList();

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

        [SlashCommand("remove", "Hard unban a twitch user from the livestream channel")]
        public async Task RemoveTwitchBanAsync(InteractionContext context, [Option("RemainingText", "RemainingText")] string username)
        {
            var bans = (Config.TwitchUserBans ?? Array.Empty<string>()).ToList();

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

        [SlashCommand("list", "Get a list of Twitch users hard banned from the livestream channel")]
        public async Task ListTwitchBanAsync(InteractionContext context)
        {
            var bans = Config.TwitchUserBans ?? Array.Empty<string>();

            var banUsernameTasks =
                bans.Select(async x => await StreamMonitorService.TwitchApiService.GetStreamerNameAsync(x));
            var usernames = await Task.WhenAll(banUsernameTasks);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Twitch Banned IDs",
                Description = Formatter.Sanitize(string.Join(Environment.NewLine, usernames)),
                Color = MomentumColor.Blue
            }.Build();

            await context.CreateResponseAsync(embed: embed);
        }

        [SlashCommand("softlist", "Get a list of Twitch users soft banned from the livestream channel")]
        public async Task ListTwitchSoftBansAsync(InteractionContext context)
        {
            var banUsernameTasks = StreamMonitorService.StreamSoftBanList.Select(async x =>
                await StreamMonitorService.TwitchApiService.GetStreamerNameAsync(x));

            var usernames = await Task.WhenAll(banUsernameTasks);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Twitch Soft Banned IDs",
                Description = Formatter.Sanitize(string.Join(Environment.NewLine, usernames)),
                Color = MomentumColor.Blue
            }.Build();

            await context.CreateResponseAsync(embed: embed);
        }
    }
}
