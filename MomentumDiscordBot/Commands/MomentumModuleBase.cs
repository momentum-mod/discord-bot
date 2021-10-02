using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Serilog;

namespace MomentumDiscordBot.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class MomentumModuleBase : ApplicationCommandModule
    {
        public ILogger Logger { get; set; }

        protected async Task<DiscordMessage> ReplyNewEmbedAsync(CommandContext context, string text, DiscordColor color)
        {
            var embed = new DiscordEmbedBuilder
            {
                Description = text,
                Color = color
            }.Build();

            return await context.RespondAsync(embed: embed);
        }

        protected async Task<DiscordMessage> SlashReplyNewEmbedAsync(InteractionContext context, string text, DiscordColor color)
        {
            await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var embed = new DiscordEmbedBuilder
            {
                Description = text,
                Color = color
            }.Build();

            return await context.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(embed));
        }
    }
}