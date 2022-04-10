using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Commands.General
{
    public class GeneralModule : MomentumModuleBase
    {
        public Configuration Config { get; set; }
    }
}