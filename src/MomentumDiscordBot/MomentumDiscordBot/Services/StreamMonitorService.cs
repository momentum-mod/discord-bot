using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace MomentumDiscordBot.Services
{
    /// <summary>
    /// Service to provide a list of current streamers playing Momentum Mod.
    /// </summary>
    public class StreamMonitorService
    {
        private Timer _intervalFunctionTimer;
        private readonly DiscordSocketClient _discordClient;
        private readonly TwitchApiService _twitchApiService;
        private readonly SocketTextChannel _textChannel;
        public StreamMonitorService(DiscordSocketClient discordClient, TimeSpan updateInterval, ulong channelId)
        {
            _discordClient = discordClient;
            _twitchApiService = new TwitchApiService();
            _textChannel = _discordClient.GetChannel(channelId) as SocketTextChannel;

            _intervalFunctionTimer = new Timer(UpdateCurrentStreamersAsync, null, TimeSpan.Zero, updateInterval);
            DeleteAllChannelEmbedsAsync().GetAwaiter().GetResult();
        }

        private async void UpdateCurrentStreamersAsync(object state)
        {
            await DeleteAllChannelEmbedsAsync();
            
            var streams = await _twitchApiService.GetLiveMomentumModStreamersAsync();

            var embedTasks = streams.Select(async x => new EmbedBuilder
            {
                Title = x.Title,
                Color = Color.Purple,
                Author = new EmbedAuthorBuilder { Name = x.UserName, IconUrl = await _twitchApiService.GetStreamerIconUrlAsync(x.UserId), Url = $"https://twitch.tv/{x.UserName}"},
                ImageUrl = x.ThumbnailUrl.Replace("{width}", "1280").Replace("{height}", "720"),
                Description = x.ViewerCount + " viewers",
                Url = $"https://twitch.tv/{x.UserName}"
            }.Build());

            var embeds = await Task.WhenAll(embedTasks);

            foreach (var embed in embeds)
            {
                await _textChannel.SendMessageAsync(embed: embed);
            }
        }

        private async Task DeleteAllChannelEmbedsAsync()
        {
            try
            {
                // Delete current messages
                var messages = await _textChannel.GetMessagesAsync().FlattenAsync();

                // Delete existing bot messages simultaneously
                var deleteTasks = messages.Where(x => x.Author.Id == _discordClient.CurrentUser.Id)
                    .Select(async x => await x.DeleteAsync());
                await Task.WhenAll(deleteTasks);
            }
            catch
            {
                // Could have old messages, safe to ignore
            }
        }
    }
}
