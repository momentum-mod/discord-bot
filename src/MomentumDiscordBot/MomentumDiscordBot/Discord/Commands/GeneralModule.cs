using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Discord.Commands
{
    public class GeneralModule : MomentumModuleBase
    {
        public CommandService CommandService { get; set; }
        public IServiceProvider Services { get; set; }
        [Command("help")]
        [Summary("The command you are running")]
        public async Task HelpAsync()
        {
            var message = await ReplyNewEmbedAsync("Building the help command... This message will be deleted when all help messages are sent", Color.Blue);
            foreach (var module in CommandService.Modules.Where(x => !x.Name.Contains("ModuleBase")))
            {
                var moduleHelpEmbed = HelpCommandUtilities.GetModuleHelpEmbed(module, Context, Services);
                if (moduleHelpEmbed.Fields.Length > 0)
                {
                    await ReplyAsync(embed: moduleHelpEmbed);

                }
            }
            await message.DeleteAsync();
        }
    }
}
