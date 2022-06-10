using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Commands.Checks;
using MomentumDiscordBot.Commands.Autocomplete;

namespace MomentumDiscordBot.Commands.General
{
    [RequireUserTrustedRole]
    public class GeneralModule : MomentumModuleBase
    {
        public Configuration Config { get; set; }

        public const string SayCommandName = "say";
        [SlashCommand(SayCommandName, "executes a custom command")]
        public async Task ExecCustomCommandAsync(InteractionContext context, [Autocomplete(typeof(AutoCompleteProvider))][Option("option", "name of the custom command")] string name)
        {
            CustomCommand command;
            if (Config.CustomCommands.TryGetValue(name, out command))
            {
                if (string.IsNullOrWhiteSpace(command.Title) && string.IsNullOrWhiteSpace(command.Description))
                {
                    //discord refuses to send messages without content
                    command.Title = "<title here!>";
                }
                var embedBuilder = new DiscordEmbedBuilder
                {
                    Title = command.Title,
                    Description = command.Description,
                    Color = MomentumColor.Blue
                };
                if (Uri.IsWellFormedUriString(command.ThumbnailUrl, UriKind.Absolute))
                {
                    embedBuilder.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = command.ThumbnailUrl,
                        Height = 90,
                        Width = 160
                    };
                }
                var message = new DiscordMessageBuilder()
                                .AddEmbed(embedBuilder.Build());

                if (Uri.IsWellFormedUriString(command.ButtonUrl, UriKind.Absolute))
                    message.AddComponents(new DiscordLinkButtonComponent(command.ButtonUrl, command.ButtonLabel ?? "link"));
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder(message));
            }
            else
            {
                await ReplyNewEmbedAsync(context, $"Command '{name}' doesn't exist", MomentumColor.Red);
            }
        }
    }
}
