using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TwitchLib.Api.Helix.Models.Streams;

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
        private Dictionary<string, ulong> _cachedStreamsIds;
        public StreamMonitorService(DiscordSocketClient discordClient, TimeSpan updateInterval, ulong channelId)
        {
            _discordClient = discordClient;
            _twitchApiService = new TwitchApiService();
            _textChannel = _discordClient.GetChannel(channelId) as SocketTextChannel;

            _cachedStreamsIds = new Dictionary<string, ulong>();

            _intervalFunctionTimer = new Timer(UpdateCurrentStreamersAsync, null, TimeSpan.Zero, updateInterval);
            DeleteAllChannelEmbedsAsync().GetAwaiter().GetResult();
        }

        private async void UpdateCurrentStreamersAsync(object state)
        {
            var streams = await _twitchApiService.GetLiveMomentumModStreamersAsync();
            var streamIds = streams.Select(x => x.Id);

            var endedStreams = _cachedStreamsIds.Where(x => !streamIds.Contains(x.Key));
            foreach (var (endedStreamId, messageId) in endedStreams)
            {
                await _textChannel.DeleteMessageAsync(messageId);
                _cachedStreamsIds.Remove(endedStreamId);
            }

            var newStreams = streams.Where(x => !_cachedStreamsIds.ContainsKey(x.Id)).ToList();
            foreach (var newStream in newStreams)
            {
                var embed = new EmbedBuilder
                {
                    Title = newStream.Title,
                    Color = Color.Purple,
                    Author = new EmbedAuthorBuilder
                    {
                        Name = newStream.UserName,
                        IconUrl = await _twitchApiService.GetStreamerIconUrlAsync(newStream.UserId),
                        Url = $"https://twitch.tv/{newStream.UserName}"
                    },
                    ImageUrl = newStream.ThumbnailUrl.Replace("{width}", "1280").Replace("{height}", "720"),
                    Description = newStream.ViewerCount + " viewers",
                    Url = $"https://twitch.tv/{newStream.UserName}"
                }.Build();

                var message = await _textChannel.SendMessageAsync(embed: embed);

                _cachedStreamsIds.Add(newStream.Id, message.Id);
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
