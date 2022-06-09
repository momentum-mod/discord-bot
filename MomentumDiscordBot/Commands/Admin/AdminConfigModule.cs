using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using HiddenAttribute = MomentumDiscordBot.Models.HiddenAttribute;

namespace MomentumDiscordBot.Commands.Admin
{
    [SlashCommandGroup("config", "show and modify the bot config")]
    public class AdminConfigModule : AdminModuleBase
    {
        public Configuration Config { get; set; }

        [SlashCommand("ls", "List config options")]
        public async Task GetConfigOptionsAsync(InteractionContext context, [Option("search", "search")] string search = null)
        {
            var configProperties = Config.GetType().GetProperties()
                .Where(x => !x.GetCustomAttributes().Any(x => x.GetType() == typeof(HiddenAttribute))).ToArray();

            if (search != null)
            {
                configProperties = configProperties
                    .Where(x => x.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            }

            await ReplyNewEmbedAsync(context, string.Join(Environment.NewLine, configProperties.Select(x => x.Name)),
                MomentumColor.Blue);
        }

        [SlashCommand("set", "Sets config option")]
        public async Task SetConfigOptionAsync(InteractionContext context, [Option("key", "key")] string key, [Option("RemainingText", "RemainingText")] string value)
        {
            var configProperties = Config.GetType().GetProperties();

            var selectedProperty =
                configProperties.Where(x => !x.GetCustomAttributes().Any(x => x.GetType() == typeof(HiddenAttribute)))
                    .FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));

            if (selectedProperty != null)
            {
                var setter = selectedProperty.GetSetMethod();
                var setterParameters = setter.GetParameters();
                if (setterParameters.Length != 1)
                {
                    throw new Exception("Expected 1 parameter for the config setter");
                }

                var configParameterType = setterParameters[0].ParameterType;

                if (configParameterType == typeof(string))
                {
                    setter.Invoke(Config, new[] { value });
                }
                else
                {
                    try
                    {
                        var convertedValue = TypeDescriptor.GetConverter(configParameterType).ConvertFromString(value);
                        setter.Invoke(Config, new[] { convertedValue });
                    }
                    catch (FormatException)
                    {

                        await ReplyNewEmbedAsync(context, $"Can't convert '{value}' to '{selectedProperty.PropertyType}", MomentumColor.Red);
                        return;
                    }
                }

                await Config.SaveToFileAsync();
                await ReplyNewEmbedAsync(context, $"Set '{selectedProperty.Name}' to '{value}'", MomentumColor.Blue);
            }
            else
            {
                await ReplyNewEmbedAsync(context, $"No config property found for '{key}'", DiscordColor.Orange);
            }
        }

        [SlashCommand("get", "Gets config option")]
        public async Task GetConfigOptionAsync(InteractionContext context, [Option("key", "key")] string key)
        {
            var configProperty = Config.GetType().GetProperties().Where(x =>
                !x.GetCustomAttributes().Any(x => x.GetType() == typeof(HiddenAttribute)) &&
                x.Name.Contains(key, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (!configProperty.Any())
            {
                await ReplyNewEmbedAsync(context, $"Could not find a config option for '{key}'", DiscordColor.Orange);
            }
            else if (configProperty.Count > 1)
            {
                await ReplyNewEmbedAsync(context, $"More than one matching key found for '{key}'", DiscordColor.Orange);
            }
            else
            {
                await ReplyNewEmbedAsync(context,
                    Formatter.Sanitize(configProperty[0].GetGetMethod().Invoke(Config, new object[0]).ToString()),
                    MomentumColor.Blue);
            }
        }
    }
}
