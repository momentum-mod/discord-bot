using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Discord.Precondition;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Discord.Commands.Moderator
{
    [ModeratorPrecondition(Group = "Moderator")]
    public class ModeratorModule : ModeratorModuleBase
    {
        public Config Config { get; set; }

        [Command("key")]
        [Summary("Give information on obtaining a game key")]
        public async Task KeyBeggingResponseAsync(IUser user)
        {
            var embed = new EmbedBuilder
            {
                Description = Config.KeyBeggingResponse,
                Color = MomentumColor.Blue
            }.Build();

            await ReplyAsync(MentionUtils.MentionUser(user.Id), embed: embed);
        }

        [Command("ban")]
        [Summary("Bans a user, purging their messages")]
        public async Task BanAsync(IGuildUser user)
        {
            await user.BanAsync(7, $"Banned by {Context.User} using !ban");
            await ReplyNewEmbedAsync($"Banned {user}, purging their messages in the last 7 days.", MomentumColor.Red);
        }
    }
}