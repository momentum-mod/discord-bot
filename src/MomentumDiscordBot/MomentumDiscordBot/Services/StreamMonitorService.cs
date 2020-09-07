using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;
using Serilog;
using TwitchLib.Api.Helix.Models.Streams;

namespace MomentumDiscordBot.Services
{
    /// <summary>
    ///     Service to provide a list of current streamers playing Momentum Mod.
    /// </summary>
    public class StreamMonitorService
    {
        private readonly ulong _channelId;
        private readonly Config _config;
        private readonly DiscordSocketClient _discordClient;
        private readonly ILogger _logger;
        private readonly List<string> _streamSoftBanList = new List<string>();

        private readonly TimeSpan _updateInterval;
        private readonly SemaphoreSlim semaphoreSlimLock = new SemaphoreSlim(1, 1);
        public readonly TwitchApiService TwitchApiService;

        // <StreamID, MessageID>
        private Dictionary<string, ulong> _cachedStreamsIds;
        private Timer _intervalFunctionTimer;
        private SocketTextChannel _textChannel;

        public StreamMonitorService(DiscordSocketClient discordClient, Config config, ILogger logger)
        {
            _config = config;
            _discordClient = discordClient;
            _logger = logger;

            TwitchApiService = new TwitchApiService(_logger);

            _channelId = _config.MomentumModStreamerChannelId;
            _updateInterval = TimeSpan.FromMinutes(_config.StreamUpdateInterval);
            _discordClient.Ready += _discordClient_Ready;
        }

        private SocketTextChannel GetTextChannel()
        {
            if (_textChannel != null)
            {
                return _textChannel;
            }

            _textChannel = _discordClient.GetChannel(_config.MomentumModStreamerChannelId) as SocketTextChannel;
            return _textChannel;
        }

        private Task _discordClient_Ready()
        {
            _ = Task.Run(async () =>
            {
                GetTextChannel();

                // Enter and lock the semaphore, incase this occurs simultaneously with updating streams
                await semaphoreSlimLock.WaitAsync();
                await TryParseExistingEmbedsAsync();
                semaphoreSlimLock.Release();

                // When reconnects occur, this will stack update events
                // Therefore, dispose every time
                _intervalFunctionTimer?.Dispose();

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

            if (_discordClient.ConnectionState != ConnectionState.Connected)
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

            await DeleteBannedStreamsAsync(streams);
            await UnSoftbanEndedStreamsAsync(streams);
            await RegisterSoftBansAsync();

            TwitchApiService.PreviousLivestreams = streams;

            // Filter out soft/hard banned streams
            var filteredStreams = streams.Where(x => !IsSoftBanned(x) && !IsHardBanned(x)).ToList();

            // Reload embeds
            try
            {
                // If there is an exception when parsing the existing embeds, no need to continue
                // Return early when there are no streams as well, as no need to send/update
                if (!await TryParseExistingEmbedsAsync() || filteredStreams.Count == 0)
                {
                    semaphoreSlimLock.Release();
                    return;
                }

                await SendOrUpdateStreamEmbedsAsync(filteredStreams);
            }
            catch (Exception e)
            {
                _logger.Error(e, "StreamMonitorService");
            }

            semaphoreSlimLock.Release();
        }

        private async Task SendOrUpdateStreamEmbedsAsync(List<Stream> filteredStreams)
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
                    var message =
                        await GetTextChannel().SendMessageAsync(messageText, embed: embed);

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
                    var oldMessage = await GetTextChannel().GetMessageAsync(messageId);
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

        private bool IsStreamInCache(Stream stream) => _cachedStreamsIds.ContainsKey(stream.Id);

        private async Task<KeyValuePair<Embed, string>> GetStreamEmbed(Stream stream)
        {
            var messageText =
                $"{stream.UserName.EscapeDiscordChars()} has gone live! {MentionUtils.MentionRole(_config.LivestreamMentionRoleId)}";

            var embed = new EmbedBuilder
            {
                Title = stream.Title.EscapeDiscordChars(),
                Color = new Color(145, 70, 255),
                Author = new EmbedAuthorBuilder
                {
                    Name = stream.UserName,
                    IconUrl = await TwitchApiService.GetStreamerIconUrlAsync(stream.UserId),
                    Url = $"https://twitch.tv/{stream.UserName}"
                },
                ImageUrl = stream.ThumbnailUrl.Replace("{width}", "1280").Replace("{height}", "720") + "?q=" +
                           Environment.TickCount,
                Description = stream.ViewerCount + " viewers",
                Url = $"https://twitch.tv/{stream.UserName}",
                Timestamp = DateTimeOffset.Now
            }.Build();

            return new KeyValuePair<Embed, string>(embed, messageText);
        }

        private bool IsHardBanned(Stream stream) => (_config.TwitchUserBans ?? new string[0]).Contains(stream.UserId);

        private bool IsSoftBanned(Stream stream) => _streamSoftBanList.Contains(stream.Id);

        private async Task RegisterSoftBansAsync()
        {
            // Check for soft-banned stream, when a mod deletes the message
            try
            {
                var existingSelfMessages =
                    (await GetTextChannel().GetMessagesAsync(200).FlattenAsync()).FromSelf(_discordClient);
                var softBannedMessages = _cachedStreamsIds.Where(x => existingSelfMessages.All(y => y.Id != x.Value));
                _streamSoftBanList.AddRange(softBannedMessages.Select(x => x.Key));
            }
            catch (Exception e)
            {
                _logger.Warning(e, "StreamMonitorService");
            }
        }

        private async Task UnSoftbanEndedStreamsAsync(IEnumerable<Stream> streams)
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
                    await GetTextChannel().DeleteMessageAsync(messageId);
                }
                catch
                {
                    _logger.Warning("StreamMonitorService: Tried to delete message " + messageId +
                                    " but it does not exist.");
                }

                _cachedStreamsIds.Remove(endedStreamId);
            }
        }

        private async Task DeleteBannedStreamsAsync(IEnumerable<Stream> streams)
        {
            // Get streams from banned users
            if (_config.TwitchUserBans != null && _config.TwitchUserBans.Length > 0)
            {
                var bannedStreams = streams.Where(x => _config.TwitchUserBans.Contains(x.UserId));

                foreach (var bannedStream in bannedStreams)
                {
                    if (_cachedStreamsIds.TryGetValue(bannedStream.Id, out var messageId))
                    {
                        await GetTextChannel().DeleteMessageAsync(messageId);
                    }
                }
            }
        }

        private async Task<bool> TryParseExistingEmbedsAsync()
        {
            // Reset cache
            _cachedStreamsIds = new Dictionary<string, ulong>();

            // Get all messages
            var messages = (await GetTextChannel().GetMessagesAsync().FlattenAsync()).FromSelf(_discordClient).ToList();

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
                    catch (HttpException e)
                    {
                        if (e.HttpCode == HttpStatusCode.NotFound)
                        {
                            // Message wasn't found due to cache?
                            
                            // At the moment, for debugging purposes, just log the message ID
                            _logger.Warning(e, "Could not delete message {message} ({id})", message, message.Id);
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