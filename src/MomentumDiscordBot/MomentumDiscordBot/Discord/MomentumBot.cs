using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord
{
    public class MomentumBot
    {
        private readonly string _discordToken;
        private readonly DiscordSocketClient _discordClient;
        private readonly LogService _logService;
        private StreamMonitorService _streamMonitorService;
        private readonly Config _config;
        public MomentumBot(string discordToken, Config config)
        {
            _discordToken = discordToken;
            _config = config;

            var discordClientConfig = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false
            };

            _discordClient = new DiscordSocketClient(discordClientConfig);
            _discordClient.Ready += _discordClient_Ready;
            
            _logService = new LogService(_discordClient);
        }

        private Task _discordClient_Ready()
        {
            // Start updating streams
            _streamMonitorService = new StreamMonitorService(_discordClient, TimeSpan.FromMinutes(5), _config.MomentumModStreamerChannelId);

            return Task.CompletedTask;
        }

        internal async Task<Exception> RunAsync()
        {
            try
            {
                // Login and start bot
                await _discordClient.LoginAsync(TokenType.Bot, _discordToken, validateToken: true);
                await _discordClient.StartAsync();

                // Block the task indefinitely
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                return e;
            }

            return null;
        }
    }
}