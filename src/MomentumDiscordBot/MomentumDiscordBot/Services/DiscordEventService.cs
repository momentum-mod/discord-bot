using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Services
{
    public class DiscordEventService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly Config _config;
        private IChannel _joinLogChannel;
        public DiscordEventService(DiscordSocketClient discordClient, Config config)
        {
            _discordClient = discordClient;
            _config = config;
            _discordClient.UserJoined += UserJoined;
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            if (_config.JoinLogChannel != default)
            {
                _joinLogChannel ??= _discordClient.GetChannel(_config.JoinLogChannel);

                if (_joinLogChannel != null && _joinLogChannel is SocketTextChannel channel)
                {
                    var accountAge = DateTimeOffset.UtcNow - user.CreatedAt;

                    var userJoinedMessage = await channel.SendMessageAsync(
                        $"{user.Mention} {user.Username}#{user.Discriminator} joined, account was created {accountAge.ToPrettyFormat()} ago");

                    if (accountAge.TotalHours <= 24)
                    {
                        await userJoinedMessage.AddReactionAsync(Emote.Parse(_config.NewUserEmoteString));
                    }
                }
            }
        }
    }
}
