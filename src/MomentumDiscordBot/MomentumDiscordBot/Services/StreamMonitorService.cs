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
        }

        private async void UpdateCurrentStreamersAsync(object state)
        {
            // Delete current messages
            var messages = await _textChannel.GetMessagesAsync().FlattenAsync();

            // Delete existing bot messages simultaneously
            var deleteTasks = messages.Where(x => x.Author.Id == _discordClient.CurrentUser.Id)
                .Select(async x => await x.DeleteAsync());
            await Task.WhenAll(deleteTasks);

            var streams = await _twitchApiService.GetLiveMomentumModStreamersAsync();
        }
    }
}
