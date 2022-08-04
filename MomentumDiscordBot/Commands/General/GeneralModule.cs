using System;
using System.Threading.Tasks;
using System.Globalization;
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

        [SlashCommand(SayCommandName, "Executes a custom command")]
        public async Task ExecCustomCommandAsync(InteractionContext context,
            [Autocomplete(typeof(AutoCompleteProvider))][Option("option", "Name of the custom command")] string name,
            [Autocomplete(typeof(MessageAutoCompleteProvider))][Option("reply", "Reply to this message")] string replyMessageId = null)
        {
            if (Config.CustomCommands.TryGetValue(name, out CustomCommand command))
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

                if (replyMessageId is not null)
                {
                    embedBuilder.Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"{context.User.Username}#{context.User.Discriminator} used /{context.CommandName} {name}",
                        IconUrl = context.User.AvatarUrl
                    };
                }

                var message = new DiscordMessageBuilder()
                    .AddEmbed(embedBuilder.Build());

                if (Uri.IsWellFormedUriString(command.ButtonUrl, UriKind.Absolute))
                    message.AddComponents(new DiscordLinkButtonComponent(command.ButtonUrl,
                        command.ButtonLabel ?? "Link"));
                if (ulong.TryParse(replyMessageId, out ulong id))
                {
                    DiscordMessage replyMessage;
                    try
                    {
                        // check if the selected message is from this channel
                        replyMessage = await context.Channel.GetMessageAsync(id);
                    }
                    catch (DSharpPlus.Exceptions.NotFoundException)
                    {
                        await context.CreateResponseAsync(new DiscordEmbedBuilder
                        {
                            Title = $"Can't find message {id} in this channel.",
                            Color = MomentumColor.Red
                        }, true);
                        return;
                    }
                    await context.Channel.SendMessageAsync(message.WithReply(id, true));
                    await context.CreateResponseAsync(new DiscordEmbedBuilder
                    {
                        Title = $"Replied to message {id}.",
                        Description = $"{replyMessage.JumpLink}",
                        Color = MomentumColor.Blue
                    }, true);
                }
                else
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder(message));
                }
            }
            else
            {
                await ReplyNewEmbedAsync(context, $"Command '{name}' doesn't exist!", MomentumColor.Red, true);
            }
        }

        [SlashCommand("timestamp", "Prints a timestamp, then when pasted in a message will be converted to all users local timezone")]
        public static async Task TimestampCommandAsync(InteractionContext context,
            [Option("timestamp", "The time you want to convert")] string timestamp,
            [Autocomplete(typeof(TimezoneAutoCompleteProvider))] [Option("timezone", "Your local timezone")]
            string timezone)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            //set culture so we know if 06.12.2022 is june or december
            var culture = new CultureInfo(context.Interaction.Locale);
            DiscordEmbedBuilder embedBuilder;
            if (!DateTime.TryParse(timestamp, culture, DateTimeStyles.NoCurrentDateDefault, out DateTime dt))
            {
                embedBuilder = new DiscordEmbedBuilder
                {
                    Title =
                        $"Can't convert '{timestamp}'.",
                    Description = "Make sure to order dates as your would in your native language.\n" +
                                  $"This command uses https://docs.microsoft.com/en-us/dotnet/api/System.DateTime.TryParse with the locale provided by your Discord client.",
                    Color = MomentumColor.Red
                };
                await context.CreateResponseAsync(embed: embedBuilder.Build(), true);
                return;
            }
            else if (dt.Date == DateTime.MinValue)
            {
                //set "today" depending on the timezone if only time was set
                var today = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo).Date;
                var time = dt.TimeOfDay;
                dt = today + time;
            }

            var dtNew = TimeZoneInfo.ConvertTimeToUtc(dt, timeZoneInfo);
            var unixTimestamp = ((DateTimeOffset)dtNew).ToUnixTimeSeconds();
            string[] formats =
            {
                "",
                ":t",
                ":T",
                ":d",
                ":D",
                ":f",
                ":F",
                ":R",
            };
            embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Unix Timestamp for {timestamp} in {timezone}",
                Description = $"Parsed as: {dt.ToLongDateString()} {dt.ToLongTimeString()}.",
                Color = MomentumColor.Blue
            };
            foreach (string format in formats)
            {
                var discordTimestamp = $"<t:{unixTimestamp}{format}>";
                embedBuilder.AddField($"{discordTimestamp}", $"\\{discordTimestamp}");
            }

            await context.CreateResponseAsync(embed: embedBuilder.Build(), true);
        }
    }
}
