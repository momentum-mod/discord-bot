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
using MomentumDiscordBot.Commands.Autocomplete;

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

        [SlashCommand("ban", "Bans a user, purging their messages")]
        public static async Task BanAsync(InteractionContext context, [Option("member", "member")] DiscordUser user)
        {
            DiscordMember member = (DiscordMember)user;
            await member.BanAsync(7, $"Banned by {context.User} using !ban");
            await ReplyNewEmbedAsync(context, $"Banned {member}, purging their messages in the last 7 days.",
                MomentumColor.Red);
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

        [SlashCommand("membercount", "Get the number of members with a role")]
        public static async Task GetMembersWithRoleAsync(InteractionContext context, [Option("role", "role")] DiscordRole role)
        {
            var (_, guildRole) = context.Guild.Roles.FirstOrDefault(x => x.Key == role.Id);

            if (guildRole != null)
            {
                var membersWithRole = context.Guild.Members.Values.Count(x => x.Roles.Contains(guildRole));
                await ReplyNewEmbedAsync(context, $"{membersWithRole} users have {guildRole.Mention}",
                    MomentumColor.Blue);
            }
            else
            {
                await ReplyNewEmbedAsync(context, "That role does not exist in this server", DiscordColor.Orange);
            }
        }

        [SlashCommand("status", "Sets the bots status")]
        public static async Task StatusAsync(InteractionContext context,
            [Option("status", "status")] string status,
            [ChoiceProvider(typeof(ActivityTypeChoiceProvider))][Option("type", "ActivityType")] string type = null)
        {
            var activity = Enum.TryParse(type, out ActivityType activityType)
                ? new DiscordActivity(status, activityType)
                : new DiscordActivity(status);
            await context.Client.UpdateStatusAsync(activity);
            await ReplyNewEmbedAsync(context, $"Status set to '{status}'.", MomentumColor.Blue);
        }

        [SlashCommand("clearstatus", "Clears the bots status")]
        public static async Task ClearStatusAsync(InteractionContext context)
        {
            await context.Client.UpdateStatusAsync(new DiscordActivity());
            await ReplyNewEmbedAsync(context, "Status cleared.", MomentumColor.Blue);
        }
    }
}
