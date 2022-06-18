using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Serilog;

namespace MomentumDiscordBot.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    public class MomentumModuleBase : ApplicationCommandModule
    {
        public ILogger Logger { get; set; }

        protected static async Task ReplyNewEmbedAsync(InteractionContext context, [Option("text", "text")] string text, [Option("color", "color")] DiscordColor color)
        {
            var embed = new DiscordEmbedBuilder
            {
                Description = text,
                Color = color
            }.Build();

            await context.CreateResponseAsync(embed: embed);
        }

        protected static async Task<DiscordMessage> SlashReplyNewEmbedAsync(InteractionContext context, [Option("text", "text")] string text, [Option("color", "color")] DiscordColor color)
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
