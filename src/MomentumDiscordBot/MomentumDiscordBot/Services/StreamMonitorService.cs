using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MomentumDiscordBot.Models;

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
        private readonly Dictionary<string, ulong> _cachedStreamsIds;
        private readonly Config _config;
        public StreamMonitorService(DiscordSocketClient discordClient, TimeSpan updateInterval, ulong channelId, Config config)
        {
            _config = config;
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

            // If the cached stream id's isn't in the fetched stream id, it is an ended stream
            var endedStreams = _cachedStreamsIds.Where(x => !streamIds.Contains(x.Key));
            foreach (var (endedStreamId, messageId) in endedStreams)
            { 
                await _textChannel.DeleteMessageAsync(messageId);
                _cachedStreamsIds.Remove(endedStreamId);
            }

            // New streams are not in the cache
            foreach (var stream in streams)
            {
                var embed = new EmbedBuilder
                {
                    Title = stream.Title,
                    Color = Color.Purple,
                    Author = new EmbedAuthorBuilder
                    {
                        Name = stream.UserName,
                        IconUrl = await _twitchApiService.GetStreamerIconUrlAsync(stream.UserId),
                        Url = $"https://twitch.tv/{stream.UserName}"
                    },
                    ImageUrl = stream.ThumbnailUrl.Replace("{width}", "1280").Replace("{height}", "720"),
                    Description = stream.ViewerCount + " viewers",
                    Url = $"https://twitch.tv/{stream.UserName}"
                }.Build();

                if (!_cachedStreamsIds.ContainsKey(stream.Id))
                {
                    // New stream, send a new message
                    var message = await _textChannel.SendMessageAsync(text: MentionUtils.MentionRole(_config.LivestreamMentionRoleId), embed: embed);

                    _cachedStreamsIds.Add(stream.Id, message.Id);
                }
                else
                {
                    // Existing stream, update message with new information
                    if (_cachedStreamsIds.TryGetValue(stream.Id, out var messageId))
                    {
                        var oldMessage = await _textChannel.GetMessageAsync(messageId);
                        if (oldMessage is RestUserMessage oldRestMessage)
                        {
                            await oldRestMessage.ModifyAsync(x => x.Embed = embed);
                        }
                    }
                }
                
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
