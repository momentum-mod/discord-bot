using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using MomentumDiscordBot.Constants;
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
        public async Task HelpAsync(string moduleSearch = null)
        {
            await ReplyAsync("Only the messages you have permission to use in this channel are included.");
            var message = await ReplyNewEmbedAsync(
                "Building the help command... This message will be deleted when all help messages are sent",
                MomentumColor.Blue);

            var desiredModules = CommandService.Modules.Where(x => !x.Name.Contains("ModuleBase")).ToList();

            if (moduleSearch != null)
            {
                desiredModules = desiredModules
                    .Where(x => x.Name.Equals(moduleSearch, StringComparison.InvariantCultureIgnoreCase) ||
                                x.Aliases.Contains(moduleSearch, StringComparer.InvariantCultureIgnoreCase)).ToList();
            }

            foreach (var module in desiredModules)
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
            await ReplyNewEmbedAsync(Config.KeyBeggingResponse, MomentumColor.Blue);
        }
    }
}