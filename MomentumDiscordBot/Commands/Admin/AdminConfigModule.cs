using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Models;
using HiddenAttribute = MomentumDiscordBot.Models.HiddenAttribute;

namespace MomentumDiscordBot.Commands.Admin
{
    [Group("config")]
    public class AdminConfigModule : AdminModuleBase
    {
        public Configuration Config { get; set; }

        [Command("ls")]
        [DSharpPlus.CommandsNext.Attributes.Description("List config options")]
        public async Task GetConfigOptionsAsync(CommandContext context, string search = null)
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

        [Command("set")]
        [DSharpPlus.CommandsNext.Attributes.Description("Sets config option")]
        public async Task SetConfigOptionAsync(CommandContext context, string key, [RemainingText] string value)
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
                    setter.Invoke(Config, new[] {value});
                }
                else
                {
                    var convertedValue = TypeDescriptor.GetConverter(configParameterType).ConvertFromString(value);

                    setter.Invoke(Config, new[] {convertedValue});
                    await Config.SaveToFileAsync();
                }


                await ReplyNewEmbedAsync(context, $"Set '{selectedProperty.Name}' to '{value}'", MomentumColor.Blue);
            }
            else
            {
                await ReplyNewEmbedAsync(context, $"No config property found for '{key}'", DiscordColor.Orange);
            }
        }

        [Command("get")]
        [DSharpPlus.CommandsNext.Attributes.Description("Gets config option")]
        public async Task GetConfigOptionAsync(CommandContext context, string key)
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