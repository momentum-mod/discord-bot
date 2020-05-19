using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Services
{
    public class KeyBeggingService
    {
        private DiscordSocketClient _discordClient;
        private Config _config;
        public KeyBeggingService(DiscordSocketClient discordClient, Config config)
        {
            _discordClient = discordClient;
            _config = config;

            _discordClient.MessageReceived += MessageReceived;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (!(message is IUserMessage userMessage)) return;
            
            if (Regex.IsMatch(userMessage.Content, _config.KeyRegexString))
            {
                await userMessage.AddReactionAsync(new Emoji(_config.KeyEmojiString));
                var embed = new EmbedBuilder
                {
                    Description = _config.KeyBeggingResponse,
                    Color = Color.Blue
                }.Build();
                await message.Channel.SendMessageAsync(embed: embed);
            }
        }
    }
}
