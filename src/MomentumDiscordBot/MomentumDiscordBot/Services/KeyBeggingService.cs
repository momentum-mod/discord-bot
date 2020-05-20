using System;
using System.Linq;
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
        private LogService _logger;
        public KeyBeggingService(DiscordSocketClient discordClient, LogService logger, Config config)
        {
            _discordClient = discordClient;
            _logger = logger;
            _config = config;

            _discordClient.MessageReceived += MessageReceived;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (!(message is IUserMessage userMessage)) return;

            try
            {
                // First check for whitelisted roles
                if (_config.WhitelistKeyBeggingRoles != null 
                    && _config.WhitelistKeyBeggingRoles.Length > 0 
                    && message.Author is SocketGuildUser guildAuthor
                    && guildAuthor.Roles.Select(x => x.Id).Any(x => _config.WhitelistKeyBeggingRoles.Contains(x)))
                {
                    return;
                }

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
            catch (Exception e)
            {
                // If it fails, oh well
                _ = _logger.LogError("KeyBeggingService", e.ToString());
            }
            
        }
    }
}
