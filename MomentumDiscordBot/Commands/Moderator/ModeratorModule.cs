using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Commands.Moderator
{
    public class ModeratorModule : ModeratorModuleBase
    {
        public StreamMonitorService StreamMonitorService { get; set; }

        public Configuration Config { get; set; }

        [SlashCommand("updatestreams", "Force an update of Twitch livestreams")]
        public async Task ForceUpdateStreamsAsync(InteractionContext context)
        {
            StreamMonitorService.UpdateCurrentStreamersAsync(null);

            await ReplyNewEmbedAsync(context, "Updating Livestreams", MomentumColor.Blue);
        }

        [SlashCommand("bans", "Returns a list of banned users")]
        public static async Task BansAsync(InteractionContext context)
        {

            await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var bans = await context.Guild.GetBansAsync();
            if (!bans.Any())
            {
                await context.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Everyone here is unbelievably nice. 0 bans."));
                return;
            }

            string time = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string fileName = $"bans{time}.txt";

            await using var fileStream = new MemoryStream();

            //array instead of class to reduce sice (no field names)
            var data = bans.Select(x => new[]
            {
                $"{x.User.Username}#{x.User.Discriminator}",
                x.User.Id.ToString(),
                x.Reason
            });
            await System.Text.Json.JsonSerializer.SerializeAsync(fileStream, data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await fileStream.FlushAsync();
            fileStream.Seek(0, SeekOrigin.Begin);

            //Attaching the file could fail because of the size.
            //If it does we see the filesize and I don't have to catch a generic HttpRequestException
            await context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"{bans.Count} banned users. Attaching {fileStream.Length / 1000 / 1000}MB file..."));
            await context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Here is a list of all {bans.Count} banned users.")
                .AddFile(fileName, fileStream));
        }

    }
}
