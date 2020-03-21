using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Services;

namespace MomentumDiscordBot.Discord.Commands
{
    public class MomentumModuleBase : ModuleBase<SocketCommandContext>
    {
        public LogService LogService { get; set; }
        protected async Task<IUserMessage> ReplyNewEmbedAsync(string text, Color color)
        {
            var embed = new EmbedBuilder
            {
                Description = text,
                Color = color
            }.Build();

            return await ReplyAsync(embed: embed);
        }
    }
}
