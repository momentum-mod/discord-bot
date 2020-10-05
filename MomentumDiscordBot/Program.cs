using System.Threading.Tasks;
using MomentumDiscordBot.Models;
using Serilog;
using Serilog.Events;

namespace MomentumDiscordBot
{
    public static class Program
    {
        internal static void Main()
            => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
        }
    }
}