﻿using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Services
{
    [Microservice(MicroserviceType.InjectAndInitialize)]
    public class MessageHistoryService
    {
        private readonly Configuration _config;
        private readonly DiscordClient _discordClient;
        private DiscordChannel _textChannel;

        public MessageHistoryService(DiscordClient discordClient, Configuration config)
        {
            _discordClient = discordClient;
            _config = config;

            _discordClient.GuildDownloadCompleted += DiscordClient_GuildsDownloaded;
            _discordClient.MessageDeleted += DiscordClient_MessageDeleted;
            _discordClient.MessageUpdated += DiscordClient_MessageUpdated;
            _discordClient.MessagesBulkDeleted += DiscordClient_MessagesBulkDeleted;
        }

        private Task DiscordClient_MessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                foreach (var message in e.Messages)
                {
                    await HandleDeletedMessageAsync(message, true);
                }
            });

            return Task.CompletedTask;
        }

        private Task DiscordClient_GuildsDownloaded(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            var channel = _discordClient.FindChannel(_config.MessageHistoryChannel);
            if (channel.Type == ChannelType.Text)
            {
                _textChannel = channel;
            }

            return Task.CompletedTask;
        }

        private Task DiscordClient_MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                // Early exits + if an embed appears, it is just a rich URL
                if (_textChannel == null || e.Channel.Guild == null || e.Author == null ||
                    e.Author.IsSelf(_discordClient)
                    || e.Message == e.MessageBefore && e.MessageBefore.Embeds.Count == 0 && e.Message.Embeds.Count != 0)
                {
                    return;
                }

                if (e.MessageBefore != null)
                {
                    var embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = "Message Edited - Old Message Content",
                        Color = MomentumColor.Blue
                    }
                        .WithDescription(Formatter.MaskedUrl("Jump to Message", e.MessageBefore.JumpLink))
                        .AddMessageContent(e.MessageBefore);

                    await _textChannel.SendMessageAsync(embed: embedBuilder.Build());
                }
                else
                {
                    await _textChannel.SendMessageAsync("A message was updated, but it was not in cache. " +
                                                        e.Message.JumpLink);
                }
            });

            return Task.CompletedTask;
        }

        private Task DiscordClient_MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await HandleDeletedMessageAsync(e.Message);
            });

            return Task.CompletedTask;
        }

        private async Task HandleDeletedMessageAsync(DiscordMessage message, bool bulkDelete = false)
        {
            if (_textChannel == null || message != null && message.Channel.Guild == null)
            {
                return;
            }

            if (message != null)
            {
                if (message.Author?.IsBot ?? true)
                {
                    return;
                }

                var embedBuilder = new DiscordEmbedBuilder
                {
                    Title = bulkDelete ? "Message Purged" : "Message Deleted",
                    Color = bulkDelete ? MomentumColor.Red : DiscordColor.Orange
                }.AddMessageContent(message);

                await _textChannel.SendMessageAsync(embed: embedBuilder.Build());
            }
            else
            {
                await _textChannel.SendMessageAsync("A message was deleted, but it was not in cache.");
            }
        }
    }
}