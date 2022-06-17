using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Constants;
using MomentumDiscordBot.Commands.Autocomplete;
using MomentumDiscordBot.Commands.General;

namespace MomentumDiscordBot.Commands.Moderator
{
    [SlashCommandGroup("custom", "Custom commands moderators can add during runtime and print a fixed response with /say")]
    public class CustomCommandModule : ModeratorModuleBase
    {
        public Configuration Config { get; set; }

        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Add as custom command")]
        public async Task MessageMenu(ContextMenuContext context)
        {
            DiscordMessage message = context.TargetMessage;
            string name = "RENAME ME! " + message.Id;

            string title = "";
            string description = message.Content;
            string buttonUrl = null;
            string buttonLabel = null;
            string thumbnailUrl = null;

            if (message.Interaction is { Name: GeneralModule.SayCommandName })
            {
                if (message.Embeds.Any())
                {
                    var embed = message.Embeds[0];
                    title = embed.Title;
                    description = embed.Description;
                    if (embed.Thumbnail is not null)
                        thumbnailUrl = embed.Thumbnail.Url.ToString();
                }
                var component = message.Components.SelectMany(x => x.Components).FirstOrDefault(x => x is DiscordLinkButtonComponent);
                if (component is DiscordLinkButtonComponent button)
                {
                    buttonUrl = button.Url;
                    buttonLabel = button.Label;
                }
            }

            DiscordEmbedBuilder embedBuilder;
            if (Config.CustomCommands.TryAdd(name, new CustomCommand(title, description, buttonUrl, buttonLabel, thumbnailUrl, context.User.Mention)))
            {
                await Config.SaveToFileAsync();
                embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "",
                    Description = "Command '" + name
                                    + "' created from message: " + message.JumpLink
                                    + "\n.Now, rename it with '/custom rename'.",
                    Color = MomentumColor.Blue,

                };
            }
            else
            {
                embedBuilder = new DiscordEmbedBuilder
                {
                    Description = "Failed to add command",
                    Color = MomentumColor.Red
                };
            }
            await context.CreateResponseAsync(embed: embedBuilder.Build(), true);
        }

        [SlashCommand("add", "Creates a new custom commands")]
        public async Task AddCustomCommandAsync(InteractionContext context, [Option("name", "Name of the new command")] string name, [Option("title", "Embed title")] string title, [Option("description", "Embed description")] string description = null)
        {
            if (Config.CustomCommands.ContainsKey(name))
                await ReplyNewEmbedAsync(context, $"Command '{name}' already exists!", MomentumColor.Red);
            else if (Config.CustomCommands.TryAdd(name, new CustomCommand(title, description, context.User.Mention)))
            {
                await Config.SaveToFileAsync();
                await ReplyNewEmbedAsync(context, $"Command '{name}' added.", MomentumColor.Blue);
            }
            else
                await ReplyNewEmbedAsync(context, "Failed to add command.", MomentumColor.Red);
        }

        [SlashCommand("remove", "Deletes a custom commands")]
        public async Task RemoveCustomCommandAsync(InteractionContext context, [Autocomplete(typeof(AutoCompleteProvider))][Option("name", "Name of the custom command")] string name)
        {
            if (Config.CustomCommands.TryRemove(name, out _))
            {
                await Config.SaveToFileAsync();
                await ReplyNewEmbedAsync(context, $"Command '{name}' removed.", MomentumColor.Blue);
            }
            else
            {
                await ReplyNewEmbedAsync(context, "Failed to remove command.", MomentumColor.Blue);
            }
        }

        [SlashCommand("rename", "Deletes a custom commands")]
        public async Task RenameCustomCommandAsync(InteractionContext context, [Autocomplete(typeof(AutoCompleteProvider))][Option("oldName", "Name of the custom command")] string oldName, [Option("newName", "The new name")] string newName)
        {
            if (Config.CustomCommands.ContainsKey(newName))
                await ReplyNewEmbedAsync(context, "Command '" + newName + "' already exists!", MomentumColor.Red);

            else if (Config.CustomCommands.ContainsKey(oldName))
            {
                CustomCommand command;
                if (!Config.CustomCommands.TryGetValue(oldName, out command))
                    await ReplyNewEmbedAsync(context, "Failed to get old command value.", MomentumColor.Red);
                else if (!Config.CustomCommands.TryAdd(newName, command))
                    await ReplyNewEmbedAsync(context, "Failed to add new command.", MomentumColor.Red);
                else if (!Config.CustomCommands.TryRemove(oldName, out _))
                    await ReplyNewEmbedAsync(context, "Failed to remove old command.", MomentumColor.Red);
                else
                {
                    await Config.SaveToFileAsync();
                    await ReplyNewEmbedAsync(context, $"Command '{oldName}' was renamed to '{newName}'.", MomentumColor.Blue);
                }
            }
            else
            {
                await ReplyNewEmbedAsync(context, $"Command '{oldName}' doesn't exist.", MomentumColor.Red);
            }
        }

        [SlashCommand("list", "Lists all custom commands")]
        public async Task ListCustomCommandAsync(InteractionContext context, [Option("page", "Which page to show, if > 25 commands.")] long page = 1)
        {
            string title = "Info Commands";
            const int itemsPerPage = 25;
            int pages = (int)Math.Ceiling((double)Config.CustomCommands.Count() / itemsPerPage);
            IEnumerable<KeyValuePair<string, CustomCommand>> commands = Config.CustomCommands.OrderByDescending(x => x.Value.CreationTimestamp);
            if (page < 1)
                page = 1;
            else if (page > pages)
                page = pages;
            if (pages > 1)
            {
                title += $" (Page {page}/{pages})";
                commands = commands.Skip((int)(itemsPerPage * (page - 1))).Take(itemsPerPage);
            }
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = title,
                Color = MomentumColor.Blue
            };
            foreach (var command in commands)
            {
                var unixTimestamp = ((DateTimeOffset)command.Value.CreationTimestamp).ToUnixTimeSeconds();
                embedBuilder.AddField(command.Key, $"Added  <t:{unixTimestamp}:R> by {command.Value.User ?? "<unknown>"}.");
            }

            await context.CreateResponseAsync(embed: embedBuilder.Build());
        }

        [SlashCommand("edit", "Change a custom commands")]
        public async Task EditCustomCommandAsync(InteractionContext context, [Autocomplete(typeof(AutoCompleteProvider))][Option("name", "Name of the custom command")] string name, [ChoiceProvider(typeof(CustomCommandPropertyChoiceProvider))][Option("key", "What you want to change")] string key, [Option("value", "The new value")] string value = null)
        {
            if (Config.CustomCommands.TryGetValue(name, out CustomCommand command))
            {
                var commandProperties = command.GetType().GetProperties();

                var selectedProperty =
                    commandProperties.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));

                if (selectedProperty != null)
                {
                    var setter = selectedProperty.GetSetMethod();
                    var setterParameters = setter.GetParameters();
                    if (setterParameters.Length != 1)
                    {
                        throw new Exception("Expected 1 parameter for the config setter.");
                    }

                    var configParameterType = setterParameters[0].ParameterType;

                    if (configParameterType == typeof(string))
                    {
                        setter.Invoke(command, new[] { value });
                    }
                    else
                    {
                        try
                        {
                            if (value is not null)
                            {
                                var convertedValue = TypeDescriptor.GetConverter(configParameterType).ConvertFromString(value);
                                setter.Invoke(command, new[] { convertedValue });
                            }
                            else
                                setter.Invoke(command, Array.Empty<object>());
                        }
                        catch (FormatException)
                        {
                            await ReplyNewEmbedAsync(context, $"Can't convert '{value}' to '{selectedProperty.PropertyType}.", MomentumColor.Red);
                            return;
                        }

                    }
                    var buttonUrlProperty = typeof(CustomCommand).GetProperty(nameof(CustomCommand.ButtonUrl));
                    if (command.ThumbnailUrl is null
                            && selectedProperty == buttonUrlProperty
                            && Uri.IsWellFormedUriString(command.ButtonUrl, UriKind.Absolute))
                    {
                        var link = new Uri(command.ButtonUrl);
                        if (link.Host == "www.youtube.com")
                        {
                            var query = System.Web.HttpUtility.ParseQueryString(link.Query);
                            string id = query["v"];
                            if (id is not null)
                                command.ThumbnailUrl = $"https://img.youtube.com/vi/{id}/mqdefault.jpg";
                        }
                    }

                    await Config.SaveToFileAsync();
                    await ReplyNewEmbedAsync(context, $"Set '{selectedProperty.Name}' to '{value}'.", MomentumColor.Blue);
                }
                else
                {
                    await ReplyNewEmbedAsync(context, $"No config property found for '{key}'.", DiscordColor.Orange);
                }
            }
            else
            {
                await ReplyNewEmbedAsync(context, $"Command '{name}' doesn't exist.", MomentumColor.Red);
            }
        }
        [SlashCommand("info", "Prints command properties")]
        public async Task InfoCustomCommandAsync(InteractionContext context, [Autocomplete(typeof(AutoCompleteProvider))][Option("name", "Name of the custom command")] string name)
        {
            if (Config.CustomCommands.TryGetValue(name, out CustomCommand command))
            {
                var embedBuilder = new DiscordEmbedBuilder
                {
                    Title = $"Command '{name}' properties",
                    Color = MomentumColor.Blue,

                };
                var commandProperties = command.GetType().GetProperties();
                foreach (var property in commandProperties)
                {
                    object value = property.GetValue(command, null);
                    string valueStr = value is null ? "<null>" : value.ToString();
                    embedBuilder.AddField(property.Name, $"'{valueStr}'");
                }
                await context.CreateResponseAsync(embed: embedBuilder.Build());
            }
            else
            {
                await ReplyNewEmbedAsync(context, $"Command '{name}' doesn't exist.", MomentumColor.Red);
            }
        }
    }
}
