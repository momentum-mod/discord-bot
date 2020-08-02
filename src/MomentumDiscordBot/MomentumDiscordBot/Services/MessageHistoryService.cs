using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Services
{
    public class MessageHistoryService
    {
        private readonly Config _config;
        private readonly DiscordSocketClient _discordClient;
        private ITextChannel _textChannel;

        public MessageHistoryService(DiscordSocketClient discordClient, Config config)
        {
            _discordClient = discordClient;
            _config = config;

            _discordClient.Ready += _discordClient_Ready;
            _discordClient.MessageDeleted += MessageDeleted;
            _discordClient.MessageUpdated += MessageUpdated;
        }

        private Task _discordClient_Ready()
        {
            if (_discordClient.GetChannel(_config.MessageHistoryChannel) is ITextChannel textChannel)
            {
                _textChannel = textChannel;
            }

            return Task.CompletedTask;
        }

        private Task MessageUpdated(Cacheable<IMessage, ulong> cachedMessage, SocketMessage newMessage,
            ISocketMessageChannel channel)
        {
            if (_textChannel == null || !(channel is IGuildChannel) || newMessage.Author.IsBot ||
                !newMessage.EditedTimestamp.HasValue)
            {
                return Task.CompletedTask;
            }

            _ = Task.Run(async () =>
            {
                if (cachedMessage.HasValue)
                {
                    var oldMessage = cachedMessage.Value;

                    var embedBuilder = new EmbedBuilder
                        {
                            Title = "Message Edited - Old Message Content",
                            Color = MomentumColor.Blue
                        }
                        .WithDescription(Format.Url("Jump to Message", oldMessage.GetJumpUrl()))
                        .AddMessageContent(oldMessage);

                    await _textChannel.SendMessageAsync(embed: embedBuilder.Build());
                }
                else
                {
                    var message = await cachedMessage.GetOrDownloadAsync();
                    if (message.Author.IsBot)
                    {
                        return;
                    }

                    await _textChannel.SendMessageAsync("A message was updated, but it was not in cache. " +
                                                        newMessage.GetJumpUrl());
                }
            });

            return Task.CompletedTask;
        }

        private Task MessageDeleted(Cacheable<IMessage, ulong> cachedMessage, ISocketMessageChannel channel)
        {
            if (_textChannel == null || !(channel is IGuildChannel))
            {
                return Task.CompletedTask;
            }

            _ = Task.Run(async () =>
            {
                if (cachedMessage.HasValue)
                {
                    if (cachedMessage.Value.Author.IsBot)
                    {
                        return;
                    }

                    var embedBuilder = new EmbedBuilder
                    {
                        Title = "Message Deleted",
                        Color = Color.Orange
                    }.AddMessageContent(cachedMessage.Value);

                    await _textChannel.SendMessageAsync(embed: embedBuilder.Build());
                }
                else
                {
                    await _textChannel.SendMessageAsync("A message was deleted, but it was not in cache.");
                }
            });

            return Task.CompletedTask;
        }
    }
}