using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Services;
using Serilog;

namespace MomentumDiscordBot.Discord.Commands
{
    public class MomentumModuleBase : ModuleBase<SocketCommandContext>
    {
        public ILogger Logger { get; set; }

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