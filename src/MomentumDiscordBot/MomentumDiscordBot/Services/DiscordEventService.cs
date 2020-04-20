using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Services
{
    public class DiscordEventService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly Config _config;
        public DiscordEventService(DiscordSocketClient discordClient, Config config)
        {
            _discordClient = discordClient;
            _config = config;

            _discordClient.Ready += _discordClient_Ready;
        }

        private Task _discordClient_Ready()
        {
            _discordClient.UserJoined += UserJoined;

            // Only subscribe once
            _discordClient.Ready -= _discordClient_Ready;

            return Task.CompletedTask;
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            if (_config.JoinLogChannel != default)
            {
                if (_discordClient.GetChannel(_config.JoinLogChannel) is SocketTextChannel channel)
                {
                    await channel.SendMessageAsync(
                        $"{user.Mention} {user.Username}#{user.Discriminator} joined, account was created {(DateTimeOffset.UtcNow - user.CreatedAt).ToPrettyFormat()}ago");
                }
            }
        }
    }
}
