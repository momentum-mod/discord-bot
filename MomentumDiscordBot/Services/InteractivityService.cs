using System;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Services
{
    [Microservice(MicroserviceType.InjectAndInitialize)]
    public class InteractivityService
    {
        private readonly Configuration _config;
        private readonly DiscordClient _discordClient;
        private DiscordChannel _textChannel;
        public InteractivityService(Configuration config, DiscordClient discordClient, IServiceProvider services)
        {
            discordClient.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromSeconds(10),
                AckPaginationButtons = true,
                ResponseBehavior = InteractionResponseBehavior.Respond,
                ResponseMessage = "Sorry, but this wasn't a valid option, or does not belong to you!",
            });

            _config = config;
            _discordClient = discordClient;
        }
    }
}
