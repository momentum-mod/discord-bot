using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;
using Serilog;

namespace MomentumDiscordBot.Services
{
    [Microservice(MicroserviceType.InjectAndInitialize)]
    public class KeyBeggingService
    {
        private readonly Configuration _config;
        private readonly DiscordClient _discordClient;
        private readonly ILogger _logger;

        public KeyBeggingService(DiscordClient discordClient, ILogger logger, Configuration config)
        {
            _discordClient = discordClient;
            _logger = logger;
            _config = config;

            _discordClient.MessageCreated += _discordClient_MessageCreated;
        }

        private Task _discordClient_MessageCreated(MessageCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                // Early return when relevant config isn't set
                if (string.IsNullOrWhiteSpace(_config.KeyBeggingResponse) ||
                    string.IsNullOrWhiteSpace(_config.KeyRegexString) ||
                    !e.Message.IsUserMessage())
                {
                    return;
                }

                try
                {
                    // First check for whitelisted roles
                    if (_config.WhitelistKeyBeggingRoles != null
                        && _config.WhitelistKeyBeggingRoles.Length > 0
                        && e.Message.Author is DiscordMember member
                        && member.Roles.Select(x => x.Id).Any(x => _config.WhitelistKeyBeggingRoles.Contains(x)))
                    {
                        return;
                    }

                    if (Regex.IsMatch(e.Message.Content, _config.KeyRegexString))
                    {
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(_config.KeyEmojiString));
                        var embed = new DiscordEmbedBuilder
                        {
                            Description = _config.KeyBeggingResponse,
                            Color = MomentumColor.Blue
                        }.Build();
                        await e.Message.Channel.SendMessageAsync(e.Author.Mention, embed: embed);
                    }
                }
                catch (Exception exception)
                {
                    // If it fails, oh well
                    _logger.Error(exception, "KeyBeggingService");
                }
            });

            return Task.CompletedTask;
        }
    }
}