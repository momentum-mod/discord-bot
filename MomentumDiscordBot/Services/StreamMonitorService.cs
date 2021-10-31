using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;
using Serilog;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace MomentumDiscordBot.Services
{
    /// <summary>
    ///     Service to provide a list of current streamers playing Momentum Mod.
    /// </summary>
    [Microservice(MicroserviceType.InjectAndInitialize)]
    public class StreamMonitorService
    {
        private readonly ulong _channelId;
        private readonly Configuration _config;
        private readonly DiscordClient _discordClient;
        private readonly ILogger _logger;
        private readonly List<string> _streamSoftBanList = new List<string>();

        public List<string> StreamSoftBanList => _streamSoftBanList;

        private readonly TimeSpan _updateInterval;
        private readonly SemaphoreSlim semaphoreSlimLock = new SemaphoreSlim(1, 1);
        public readonly TwitchApiService TwitchApiService;

        // <StreamID, MessageID>
        private Dictionary<string, ulong> _cachedStreamsIds;

        private bool _discordClientConnected;
        private Timer _intervalFunctionTimer;
        private DiscordChannel _textChannel;

        public StreamMonitorService(DiscordClient discordClient, TwitchApiService twitchApiService,
            Configuration config, ILogger logger)
        {
            _config = config;
            _discordClient = discordClient;
            _logger = logger;

            TwitchApiService = twitchApiService;

            _channelId = _config.MomentumModStreamerChannelId;
            _updateInterval = TimeSpan.FromMinutes(_config.StreamUpdateInterval);
            _discordClient.GuildDownloadCompleted += _discordClient_GuildsDownloaded;

            _discordClient.SocketOpened += (s, e) =>
            {
                _discordClientConnected = true;
                return Task.CompletedTask;
            };

            _discordClient.SocketClosed += (s, e) =>
            {
                _discordClientConnected = false;
                return Task.CompletedTask;
            };
        }

        private void UpdateTextChannel()
        {
            _textChannel = _discordClient.FindChannel(_config.MomentumModStreamerChannelId);
        }

        private Task _discordClient_GuildsDownloaded(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                UpdateTextChannel();

                // Enter and lock the semaphore, incase this occurs simultaneously with updating streams
                await semaphoreSlimLock.WaitAsync();
                var messages = (await _textChannel.GetMessagesAsync()).ToList();
                await TryParseExistingEmbedsAsync(messages);
                semaphoreSlimLock.Release();

                // When reconnects occur, this will stack update events
                // Therefore, dispose every time
                if (_intervalFunctionTimer != null)
                {
                    await _intervalFunctionTimer.DisposeAsync();
                }

                _intervalFunctionTimer = new Timer(UpdateCurrentStreamersAsync, null, TimeSpan.Zero, _updateInterval);
            });

            return Task.CompletedTask;
        }

        public async void UpdateCurrentStreamersAsync(object state)
        {
            _logger.Verbose("Waiting to enter UpdateCurrentStreamersAsync...");
            // Wait for the semaphore to unlock, then lock it
            await semaphoreSlimLock.WaitAsync();
            _logger.Verbose("Entered UpdateCurrentStreamersAsync");

            if (!_discordClientConnected)
            {
                semaphoreSlimLock.Release();
                return;
            }

            var streams = await TwitchApiService.GetLiveMomentumModStreamersAsync();

            // On error no need to continue
            if (streams == null)
            {
                semaphoreSlimLock.Release();
                return;
            }

            UpdateTextChannel();

            var messages = (await _textChannel.GetMessagesAsync()).ToList();

            await DeleteBannedStreamsAsync(streams, messages);
            await UnSoftbanEndedStreamsAsync(streams, messages);
            RegisterSoftBans(messages);

            TwitchApiService.PreviousLivestreams = streams;

            // Filter out soft/hard banned streams
            var filteredStreams = streams.Where(x => !IsSoftBanned(x) && !IsHardBanned(x)).ToList();

            // Reload embeds
            try
            {
                // If there is an exception when parsing the existing embeds, no need to continue
                // Return early when there are no streams as well, as no need to send/update
                if (!await TryParseExistingEmbedsAsync(messages) || filteredStreams.Count == 0)
                {
                    semaphoreSlimLock.Release();
                    return;
                }

                await SendOrUpdateStreamEmbedsAsync(filteredStreams, messages);
            }
            catch (Exception e)
            {
                _logger.Error(e, "StreamMonitorService");
            }

            semaphoreSlimLock.Release();
        }

        private async Task SendOrUpdateStreamEmbedsAsync(List<Stream> filteredStreams, List<DiscordMessage> messages)
        {
            foreach (var stream in filteredStreams)
            {
                var (embed, messageText) = await GetStreamEmbed(stream);

                // New streams are not in the cache
                if (!IsStreamInCache(stream))
                {
                    // If the stream is not above the minimum viewers then ignore it, but we want to update a stream if it dips below
                    if (stream.ViewerCount < _config.MinimumStreamViewersAnnounce)
                    {
                        continue;
                    }

                    // New stream, send a new message
                    var messageBuilder = new DiscordMessageBuilder()
                    {
                        Content = messageText,
                        Embed = embed,
                        Mentions =
                        {
                            new RoleMention(_config.LivestreamMentionRoleId)
                        }
                    };

                    var message =
                        await _textChannel.SendMessageAsync(messageBuilder);

                    _cachedStreamsIds.Add(stream.Id, message.Id);
                }
                else
                {
                    // Get the message id from the stream
                    if (!_cachedStreamsIds.TryGetValue(stream.Id, out var messageId))
                    {
                        _logger.Warning("StreamMonitorService: Could not message from cached stream ID");
                        continue;
                    }

                    // Existing stream, update message with new information
                    var oldMessage = messages.FirstOrDefault(x => x.Id == messageId);

                    if (oldMessage == null)
                    {
                        continue;
                    }

                    if (oldMessage.Author.IsSelf(_discordClient))
                    {
                        await oldMessage.ModifyAsync(messageText, embed);
                    }
                }
            }
        }

        private bool IsStreamInCache(Stream stream) => _cachedStreamsIds.ContainsKey(stream.Id);

        private async Task<KeyValuePair<DiscordEmbed, string>> GetStreamEmbed(Stream stream)
        {
            var mentionRole = _discordClient.FindRole(_config.LivestreamMentionRoleId);
            var messageText =
                $"{Formatter.Sanitize(stream.UserName)} has gone live! {mentionRole.Mention}";

            var embed = new DiscordEmbedBuilder
                {
                    Title = Formatter.Sanitize(stream.Title),
                    Color = new DiscordColor(145, 70, 255),
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = stream.UserName,
                        IconUrl = await TwitchApiService.GetStreamerIconUrlAsync(stream.UserId),
                        Url = $"https://twitch.tv/{stream.UserLogin}"
                    },
                    ImageUrl = stream.ThumbnailUrl.Replace("{width}", "1280").Replace("{height}", "720") + "?q=" +
                               Environment.TickCount,
                    Url = $"https://twitch.tv/{stream.UserLogin}",
                    Timestamp = DateTimeOffset.Now
                }.AddField("🔴 Viewers", stream.ViewerCount.ToString(), true)
                .AddField("🎦 Uptime", (DateTime.UtcNow - stream.StartedAt).ToPrettyFormat(2), true)
                .WithFooter("Streaming " + await TwitchApiService.GetGameNameAsync(stream.GameId))
                .Build();

            return new KeyValuePair<DiscordEmbed, string>(embed, messageText);
        }

        private bool IsHardBanned(Stream stream) => (_config.TwitchUserBans ?? new string[0]).Contains(stream.UserId);

        private bool IsSoftBanned(Stream stream) => _streamSoftBanList.Contains(stream.Id);

        private void RegisterSoftBans(List<DiscordMessage> messages)
        {
            // Check for soft-banned stream, when a mod deletes the message
            try
            {
                var existingSelfMessages =
                    messages.FromSelf(_discordClient);
                var softBannedMessages = _cachedStreamsIds.Where(x => existingSelfMessages.All(y => y.Id != x.Value)).ToList();

                foreach (var softBannedMessage in softBannedMessages)
                {
                    _logger.Information("Registered softban for streamer {streamId}", softBannedMessage.Key);
                }

                _streamSoftBanList.AddRange(softBannedMessages.Select(x => x.Key));
            }
            catch (Exception e)
            {
                _logger.Warning(e, "StreamMonitorService");
            }
        }

        private async Task UnSoftbanEndedStreamsAsync(IEnumerable<Stream> streams, List<DiscordMessage> messages)
        {
            // If the cached stream id's isn't in the fetched stream id, it is an ended stream
            var streamIds = streams.Select(x => x.Id);
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
                    var message = messages.FirstOrDefault(x => x.Id == messageId);

                    if (message == null)
                    {
                        continue;
                    }

                    await _textChannel.DeleteMessageAsync(message);
                    messages.Remove(message);
                }
                catch
                {
                    _logger.Warning("StreamMonitorService: Tried to delete message " + messageId +
                                    " but it does not exist.");
                }

                _cachedStreamsIds.Remove(endedStreamId);
            }
        }

        private async Task DeleteBannedStreamsAsync(IEnumerable<Stream> streams, List<DiscordMessage> messages)
        {
            // Get streams from banned users
            if (_config.TwitchUserBans != null && _config.TwitchUserBans.Length > 0)
            {
                var bannedStreams = streams.Where(x => _config.TwitchUserBans.Contains(x.UserId));

                foreach (var bannedStream in bannedStreams)
                {
                    if (_cachedStreamsIds.TryGetValue(bannedStream.Id, out var messageId))
                    {
                        var message = messages.FirstOrDefault(x => x.Id == messageId);

                        if (message == null)
                        {
                            continue;
                        }

                        await _textChannel.DeleteMessageAsync(message);

                        messages.Remove(message);
                    }
                }
            }
        }

        private async Task<bool> TryParseExistingEmbedsAsync(List<DiscordMessage> messages)
        {
            // Reset cache
            _cachedStreamsIds = new Dictionary<string, ulong>();

            // Get all messages
            messages = messages.FromSelf(_discordClient).ToList();

            if (!messages.Any())
            {
                return true;
            }

            var streams = await TwitchApiService.GetLiveMomentumModStreamersAsync();

            // Error getting streams, don't continue
            if (streams == null)
            {
                return false;
            }

            // Delete existing bot messages simultaneously
            var deleteTasks = messages
                .Select(async message =>
                {
                    try
                    {
                        if (message.Embeds.Count == 1)
                        {
                            var matchingStream =
                                streams.FirstOrDefault(y => y.UserName == message.Embeds.First().Author?.Name);
                            if (matchingStream == null)
                            {
                                // No matching stream
                                await message.DeleteAsync();
                            }
                            else if (!_cachedStreamsIds.TryAdd(matchingStream.Id, message.Id))
                            {
                                // Found the matching stream
                                _logger.Warning("StreamMonitorService: Duplicate cached streamer: " +
                                                matchingStream.UserName + ", deleting...");
                                await message.DeleteAsync();
                            }
                        }
                        else
                        {
                            // Stream has ended, or failed to parse
                            await message.DeleteAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Warning(e, "Could not delete message {message}", message);
                    }
                });

            await Task.WhenAll(deleteTasks);
            return true;
        }
    }
}
