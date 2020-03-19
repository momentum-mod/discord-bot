using System;
using System.IO;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Discord;
using MomentumDiscordBot.Models;
using Newtonsoft.Json;

namespace MomentumDiscordBot
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Loading discord token...");
            if (!TryGetDiscordToken(out var discordToken))
            {
                // No token, quit
                return;
            }

            Console.WriteLine("Loading config file...");
            if (!TryGetConfig(out var config))
            {
                // No config, quit
                return;
            }

            var bot = new MomentumBot(discordToken, config);

            var closingException = bot.RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            // Log unsafe exits, have to do this explicitly in async context
            if (closingException == null)
            {
                Console.WriteLine("Application quitting safely.");
            }
            else
            {
                Console.WriteLine("Application quitting unsafely, exception information:");
                Console.WriteLine(closingException.ToString());
            }
        }

        private static bool TryGetDiscordToken(out string discordToken)
        {
            if (File.Exists(PathConstants.DiscordTokenFilePath))
            {
                // File exists, get the text
                discordToken = File.ReadAllText(PathConstants.DiscordTokenFilePath);
                return true;
            }
            else
            {
                Console.WriteLine(
                    $"No discord token file exists, expected it at: '{PathConstants.DiscordTokenFilePath}'");

                discordToken = null;
                return false;
            }
        }

        private static bool TryGetConfig(out Config config)
        {
            if (File.Exists(PathConstants.ConfigFilePath))
            {
                // File exists, get the text
                var configString = File.ReadAllText(PathConstants.ConfigFilePath);
                config = JsonConvert.DeserializeObject<Config>(configString);
                return true;
            }
            else
            {
                Console.WriteLine(
                    $"No config file exists, expected it at: '{PathConstants.ConfigFilePath}'");

                config = null;
                return false;
            }
        }
    }
}
