﻿using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Commands.General
{
    public class GeneralModule : MomentumModuleBase
    {
        public Configuration Config { get; set; }

        [Command("key")]
        [Description("Give information on obtaining a game key")]
        public async Task KeyBeggingResponseAsync(CommandContext context)
        {
            await ReplyNewEmbedAsync(context, Config.KeyBeggingResponse, MomentumColor.Blue);
        }
    }
}