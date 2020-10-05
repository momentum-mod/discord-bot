using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Commands.Moderator
{
    public class ModeratorModule : ModeratorModuleBase
    {
        public Configuration Config { get; set; }


        [Command("ban")]
        [Description("Bans a user, purging their messages")]
        public async Task BanAsync(CommandContext context, DiscordMember member)
        {
            await member.BanAsync(7, $"Banned by {context.User} using !ban");
            await ReplyNewEmbedAsync(context, $"Banned {member}, purging their messages in the last 7 days.",
                MomentumColor.Red);
        }
    }
}