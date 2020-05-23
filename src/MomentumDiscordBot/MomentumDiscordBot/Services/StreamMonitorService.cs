using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;
using TwitchLib.Api.Helix.Models.Streams;

namespace MomentumDiscordBot.Services
{
    /// <summary>
    ///     Service to provide a list of current streamers playing Momentum Mod.
    /// </summary>
    public class StreamMonitorService
    {
        private ulong _channelId;
        private TimeSpan _updateInterval;
        private Dictionary<string, ulong> _cachedStreamsIds;
        private readonly Config _config;
        private readonly DiscordSocketClient _discordClient;
        private SocketTextChannel _textChannel;
        public readonly TwitchApiService TwitchApiService;
        private Timer _intervalFunctionTimer;
        private List<string> _streamSoftBanList = new List<string>();
        private List<Stream> _previousStreams;
        private LogService _logger;
        private SemaphoreSlim semaphoreSlimLock = new SemaphoreSlim(1, 1);

        public StreamMonitorService(DiscordSocketClient discordClient, Config config, LogService logger)
        {
            _config = config;
            _discordClient = discordClient;
            _logger = logger;

            TwitchApiService = new TwitchApiService(_logger);

            _channelId = _config.MomentumModStreamerChannelId;
            _updateInterval = TimeSpan.FromMinutes(_config.StreamUpdateInterval);
            _discordClient.Ready += _discordClient_Ready;
        }

        private Task _discordClient_Ready()
        {
            _ = Task.Run(async () => 
            {
                _textChannel = _discordClient.GetChannel(_channelId) as SocketTextChannel;

                await TryParseExistingEmbedsAsync();

                _intervalFunctionTimer = new Timer(UpdateCurrentStreamersAsync, null, TimeSpan.Zero, _updateInterval);
            });

            return Task.CompletedTask;
        }

        public async void UpdateCurrentStreamersAsync(object state)
        {
            // Wait for the semaphore to unlock, then lock it
            await semaphoreSlimLock.WaitAsync();

            var streams = await TwitchApiService.GetLiveMomentumModStreamersAsync();

            if (streams == null || streams.Count == 0) return;

            var streamIds = streams.Select(x => x.Id);

            // Get streams from banned users
            if (_config.TwitchUserBans != null && _config.TwitchUserBans.Length > 0)
            {
                var bannedStreams = streams.Where(x => _config.TwitchUserBans.Contains(x.UserId));
                foreach (var bannedStream in bannedStreams)
                {
                    if (_cachedStreamsIds.TryGetValue(bannedStream.Id, out var messageId))
                    {
                        await _textChannel.DeleteMessageAsync(messageId);
                    }
                }
            }

            // If the cached stream id's isn't in the fetched stream id, it is an ended stream
            var endedStreams = _cachedStreamsIds.Where(x => !streamIds.Contains(x.Key));
            
            foreach (var (endedStreamId, messageId) in endedStreams)
            {
                // If the stream was soft banned, remove it
                if (_streamSoftBanList.Contains(endedStreamId))
                {
                    _streamSoftBanList.Remove(endedStreamId);
                }

                try
                {
                    await _textChannel.DeleteMessageAsync(messageId);
                }
                catch
                {
                    _ = _logger.LogWarning("StreamMonitorService", "Tried to delete message " + messageId + " but it does not exist.");
                }
                
                _cachedStreamsIds.Remove(endedStreamId);
            }

            // Check for soft-banned stream, when a mod deletes the message
            try
            {
                var existingSelfMessages = (await _textChannel.GetMessagesAsync(limit: 200).FlattenAsync()).FromSelf(_discordClient);
                var softBannedMessages = _cachedStreamsIds.Where(x => existingSelfMessages.All(y => y.Id != x.Value));
                _streamSoftBanList.AddRange(softBannedMessages.Select(x => x.Key));
            }
            catch (Exception e)
            {
                _ = _logger.LogWarning("StreamMonitorService", e.Message);
            }

            _previousStreams = streams;

            // Filter out soft/hard banned streams
            var filteredStreams = streams.Where(x => !_streamSoftBanList.Contains(x.Id) && 
                                                     !(_config.TwitchUserBans ?? new string[0]).Contains(x.UserId));

            // Reload embeds
            try
            {
                await TryParseExistingEmbedsAsync();

                foreach (var stream in filteredStreams)
                {
                    var messageText =
                        $"{stream.UserName.EscapeDiscordChars()} has gone live! {MentionUtils.MentionRole(_config.LivestreamMentionRoleId)}";

                    var embed = new EmbedBuilder
                    {
                        Title = stream.Title.EscapeDiscordChars(),
                        Color = Color.Purple,
                        Author = new EmbedAuthorBuilder
                        {
                            Name = stream.UserName,
                            IconUrl = await TwitchApiService.GetStreamerIconUrlAsync(stream.UserId),
                            Url = $"https://twitch.tv/{stream.UserName}"
                        },
                        ImageUrl = stream.ThumbnailUrl.Replace("{width}", "1280").Replace("{height}", "720") + "?q=" + Environment.TickCount,
                        Description = stream.ViewerCount + " viewers",
                        Url = $"https://twitch.tv/{stream.UserName}",
                        Timestamp = DateTimeOffset.Now
                    }.Build();

                    // New streams are not in the cache
                    if (!_cachedStreamsIds.ContainsKey(stream.Id))
                    {
                        // If the stream is not above the minimum viewers then ignore it, but we want to update a stream if it dips below
                        if (stream.ViewerCount < _config.MinimumStreamViewersAnnounce) continue;

                        // New stream, send a new message
                        var message =
                            await _textChannel.SendMessageAsync(messageText, embed: embed);

                        _cachedStreamsIds.Add(stream.Id, message.Id);
                    }
                    else
                    {
                        // Existing stream, update message with new information
                        if (_cachedStreamsIds.TryGetValue(stream.Id, out var messageId))
                        {
                            var oldMessage = await _textChannel.GetMessageAsync(messageId);
                            if (oldMessage is IUserMessage oldRestMessage)
                            {
                                await oldRestMessage.ModifyAsync(x =>
                                {
                                    x.Content = messageText;
                                    x.Embed = embed;
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _ = _logger.LogError("StreamMonitorService", e.ToString());
            }

            semaphoreSlimLock.Release();
        }

        private async Task TryParseExistingEmbedsAsync()
        {
            // Reset cache
            _cachedStreamsIds = new Dictionary<string, ulong>();

            // Get all messages
            var messages = (await _textChannel.GetMessagesAsync().FlattenAsync()).FromSelf(_discordClient).ToList();

            if (!messages.Any()) return;

            // Get current streams, instead of returning early on error just delete all cache
            var streams = await TwitchApiService.GetLiveMomentumModStreamersAsync() ?? new List<Stream>();

            // Delete existing bot messages simultaneously
            var deleteTasks = messages
                .Select(async x =>
                {
                    if (x.Embeds.Count == 1)
                    {
                        var matchingStream = streams.FirstOrDefault(y => y.UserName == x.Embeds.First().Author?.Name);
                        if (matchingStream == null)
                        {
                            // No matching stream
                            await x.DeleteAsync();
                        }
                        else
                        {
                            // Found the matching stream
                            if (!_cachedStreamsIds.TryAdd(matchingStream.Id, x.Id))
                            {
                                await _logger.LogWarning("StreamMonitorService", "Duplicate cached streamer: " + matchingStream.UserName + ", deleting...");
                                await x.DeleteAsync();
                            }
                        }
                    }
                    else
                    {
                        // Stream has ended, or failed to parse
                        await x.DeleteAsync();
                    }
                });
            await Task.WhenAll(deleteTasks);
        }

        public async Task<string> GetOrDownloadTwitchIDAsync(string username)
        { 
            if (ulong.TryParse(username, out _))
            {
                // Input is a explicit Twitch ID
                return username;
            }
            else
            {
                // Input is the Twitch username
                var cachedUser = _previousStreams.FirstOrDefault(x =>
                    string.Equals(username, x.UserName, StringComparison.InvariantCultureIgnoreCase));

                if (cachedUser != null)
                {
                    // User is in the cache
                    return cachedUser.UserId;
                }

                try
                { 
                    // Search the API, throws exception if not found
                    return await TwitchApiService.GetStreamerIDAsync(username);
                }
                catch (Exception e)
                {
                    _ = _logger.LogError("StreamMonitorService", e.ToString());
                    return null;
                }
                
            }
        }
    }
}
