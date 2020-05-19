using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Discord.Commands
{
    [Group("config")]
    public class AdminConfigModule : AdminModule
    {
        public Config Config { get; set; }

        [Command("ls")]
        [Summary("List config options")]
        public async Task GetConfigOptionsAsync(string search = null)
        {
            var configProperties = Config.GetType().GetProperties();

            if (search != null)
            {
                configProperties = configProperties
                    .Where(x => x.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            }

            await ReplyNewEmbedAsync(string.Join(Environment.NewLine, configProperties.Select(x => x.Name)), Color.Blue);
        }

        [Command("set")]
        [Summary("Sets config option")]
        public async Task SetConfigOptionAsync(string key, [Remainder] string value)
        {
            var configProperties = Config.GetType().GetProperties();

            var selectedProperty =
                configProperties.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));

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
                    var convertedValue = TypeDescriptor.GetConverter(configParameterType).ConvertFromString(value);

                    setter.Invoke(Config, new[] { convertedValue });
                }


                await ReplyNewEmbedAsync($"Set '{selectedProperty.Name}' to '{value}'", Color.Blue);
            }
            else
            {
                await ReplyNewEmbedAsync($"No config property found for '{key}'", Color.Orange);
            }
        }

        [Command("get")]
        [Summary("Gets config option")]
        public async Task GetConfigOptionAsync(string key)
        {
            var configProperty = Config.GetType().GetProperties().Where(x => x.Name.Contains(key, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (!configProperty.Any())
            {
                await ReplyNewEmbedAsync($"Could not find a config option for '{key}'", Color.Orange);
            }
            else if (configProperty.Count > 1)
            {
                await ReplyNewEmbedAsync($"More than one matching key found for '{key}'", Color.Orange);
            }
            else
            {
                await ReplyNewEmbedAsync(configProperty[0].GetGetMethod().Invoke(Config, new object[0]).ToString(), Color.Blue);
            }
        }
    }
}
