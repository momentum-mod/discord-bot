using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Discord.Commands
{
    public class GeneralModule : MomentumModuleBase
    {
        public CommandService CommandService { get; set; }
        public IServiceProvider Services { get; set; }
        public Config Config { get; set; }
        [Command("help")]
        [Summary("The command you are running")]
        public async Task HelpAsync()
        {
            await ReplyAsync("Only the messages you have permission to use in this channel are included.");
            var message = await ReplyNewEmbedAsync("Building the help command... This message will be deleted when all help messages are sent", Color.Blue);
            foreach (var module in CommandService.Modules.Where(x => !x.Name.Contains("ModuleBase")))
            {
                var moduleHelpEmbed = HelpCommandUtilities.GetModuleHelpEmbed(module, Context, Services, Config);
                if (moduleHelpEmbed.Fields.Length > 0)
                {
                    await ReplyAsync(embed: moduleHelpEmbed);

                }
            }
            await message.DeleteAsync();
        }

        [Command("key")]
        [Summary("Give information on obtaining a game key")]
        public async Task KeyBeggingResponseAsync()
        {
            await ReplyNewEmbedAsync(Config.KeyBeggingResponse, Color.Blue);
        }
    }
}
