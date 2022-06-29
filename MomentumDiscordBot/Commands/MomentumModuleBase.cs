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

        protected static async Task ReplyNewEmbedAsync(InteractionContext context, string text, DiscordColor color, bool ephemeral = false)
        {
            await ReplyNewEmbedAsync(context.Interaction, text, color, ephemeral);
        }

        protected static async Task ReplyNewEmbedAsync(DiscordInteraction inter, string text, DiscordColor color, bool ephemeral = false)
        {
            var embed = new DiscordEmbedBuilder
            {
                Description = text,
                Color = color
            }.Build();

            await ReplyNewEmbedAsync(inter, embed, ephemeral);
        }

        protected static async Task ReplyNewEmbedAsync(DiscordInteraction inter, DiscordEmbed embed, bool ephemeral = false)
        {
            await inter.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed: embed).AsEphemeral(ephemeral));
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
