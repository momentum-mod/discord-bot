using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Discord.Precondition;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Discord.Commands.Moderator
{
    [ModeratorPrecondition]
    public class ModeratorModule : MomentumModuleBase
    {
        public Config Config { get; set; }

        [Command("key")]
        [Summary("Give information on obtaining a game key")]
        public async Task KeyBeggingResponseAsync(IUser user)
        {
            var embed = new EmbedBuilder
            {
                Description = Config.KeyBeggingResponse,
                Color = Color.Blue
            }.Build();

            await ReplyAsync(MentionUtils.MentionUser(user.Id), embed: embed);
        }
    }
}
