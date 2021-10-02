using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Commands.General
{
    public class GeneralModule : MomentumModuleBase
    {
        public Configuration Config { get; set; }

        [SlashCommand("key", "Give information on obtaining a game key")]
        public async Task KeyBeggingResponseAsync(InteractionContext context)
        {
            await SlashReplyNewEmbedAsync(context, Config.KeyBeggingResponse, MomentumColor.Blue);
        }
    }
}