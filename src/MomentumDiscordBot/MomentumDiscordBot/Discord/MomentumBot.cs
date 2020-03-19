using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord
{
    public class MomentumBot
    {
        private readonly string _discordToken;
        private readonly DiscordSocketClient _discordClient;
        private readonly LogService _logService;
        public MomentumBot(string discordToken)
        {
            _discordToken = discordToken;

            var discordClientConfig = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false
            };

            _discordClient = new DiscordSocketClient(discordClientConfig);

            _logService = new LogService(_discordClient);
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