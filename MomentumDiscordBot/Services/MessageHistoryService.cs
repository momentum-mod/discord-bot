using System.Threading.Tasks;
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

            _discordClient.GuildDownloadCompleted += _discordClient_GuildsDownloaded;
            _discordClient.MessageDeleted += _discordClient_MessageDeleted;
            _discordClient.MessageUpdated += _discordClient_MessageUpdated;
        }

        private Task _discordClient_GuildsDownloaded(GuildDownloadCompletedEventArgs e)
        {
            var channel = _discordClient.FindChannel(_config.MessageHistoryChannel);
            if (channel.Type == ChannelType.Text)
            {
                _textChannel = channel;
            }

            return Task.CompletedTask;
        }

        private async Task _discordClient_MessageUpdated(MessageUpdateEventArgs e)
        {
            if (_textChannel == null || e.Channel.Guild == null || e.Author == null || e.Author.IsSelf(_discordClient))
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
        }

        private async Task _discordClient_MessageDeleted(MessageDeleteEventArgs e)
        {
            if (_textChannel == null || e.Guild == null)
            {
                return;
            }

            if (e.Message != null)
            {
                if (e.Message.Author?.IsBot ?? true)
                {
                    return;
                }

                var embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "Message Deleted",
                    Color = DiscordColor.Orange
                }.AddMessageContent(e.Message);

                await _textChannel.SendMessageAsync(embed: embedBuilder.Build());
            }
            else
            {
                await _textChannel.SendMessageAsync("A message was deleted, but it was not in cache.");
            }
        }
    }
}