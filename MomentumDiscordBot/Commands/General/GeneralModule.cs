using System;
using System.Threading.Tasks;
using DSharpPlus;
using System.Globalization;
using System.Threading.Tasks;
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

        [SlashCommand("timestamp", "prints a discord timestamp")]
        public static async Task TimestampCommandAsync(InteractionContext context, [Option("timestamp", "the time you want to convert")] string timestamp, [Autocomplete(typeof(TimezoneAutoCompleteProvider))][Option("timezone", "the timezone you are in")] string timezone)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            //set culture so we know if 06.12.2022 is june or december
            var culture = new CultureInfo(context.Interaction.Locale);
            DiscordEmbedBuilder embedBuilder;
            if (!DateTime.TryParse(timestamp, culture, DateTimeStyles.NoCurrentDateDefault, out DateTime dt))
            {
                embedBuilder = new DiscordEmbedBuilder
                {
                    Title = $"Can't convert '{timestamp}'",
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
            string[] formats = {
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
                Title = $"{timestamp} {timezone}",
                Description = $"{dt.ToLongDateString()} {dt.ToLongTimeString()}",
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
