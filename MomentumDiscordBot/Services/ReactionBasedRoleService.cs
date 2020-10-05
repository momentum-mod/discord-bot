using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;
using Serilog;

namespace MomentumDiscordBot.Services
{
    [Microservice(MicroserviceType.InjectAndInitialize)]
    public class ReactionBasedRoleService
    {
        private readonly Configuration _config;

        private readonly DiscordClient _discordClient;

        // <RoleID, MessageID>
        private Dictionary<ulong, ulong> _existingRoleEmbeds;
        private DiscordChannel _textChannel;

        public ReactionBasedRoleService(DiscordClient discordClient, Configuration config)
        {
            _config = config;

            _discordClient = discordClient;
            _discordClient.GuildDownloadCompleted += _discordClient_GuildsDownloaded;
            _discordClient.MessageReactionAdded += _discordClient_MessageReactionAdded;
            _discordClient.MessageReactionRemoved += _discordClient_MessageReactionRemoved;
        }

        private async Task _discordClient_GuildsDownloaded(GuildDownloadCompletedEventArgs e)
        {
            _textChannel = _discordClient.FindChannel(_config.RolesChannelId);

            await LoadExistingRoleEmbedsAsync();
            await SendRoleEmbedsAsync();

            await VerifyCurrentUserRolesAsync();
        }

        private async Task LoadExistingRoleEmbedsAsync()
        {
            _existingRoleEmbeds = new Dictionary<ulong, ulong>();

            // Filter only messages from this bot
            var existingMessages = (await _textChannel.GetMessagesAsync()).FromSelf(_discordClient);

            foreach (var message in existingMessages)
            {
                var (result, role) = await TryParseRoleFromEmbedAsync(message);
                if (result)
                {
                    _existingRoleEmbeds.Add(role.Id, message.Id);
                }
            }
        }

        private async Task SendRoleEmbedsAsync()
        {
            // If there are roles added to the config, but aren't sent yet, send them
            if (_config.MentionRoles != null)
            {
                foreach (var mentionRole in _config.MentionRoles)
                {
                    if (_existingRoleEmbeds.ContainsKey(mentionRole))
                    {
                        continue;
                    }

                    var role = _textChannel.Guild.Roles.First(x => x.Key == mentionRole);
                    await SendRoleEmbed(role.Value);
                }
            }
        }

        private async Task VerifyCurrentUserRolesAsync()
        {
            var usersWithMentionRoles = _textChannel.Guild.Members
                .Where(x => _config.MentionRoles.Intersect(x.Value.Roles.Select(y => y.Id)).Any()).ToList();

            // Check users who have reacted to the embed
            foreach (var (roleId, messageId) in _existingRoleEmbeds)
            {
                var message = await _textChannel.GetMessageAsync(messageId);
                var role = _textChannel.Guild.GetRole(roleId);

                if (!message.Author.IsSelf(_discordClient))
                {
                    continue;
                }

                // Get all users who have reacted to the embed
                var reactionUsers =
                    (await message.GetReactionsAsync(_config.MentionRoleEmoji, _textChannel.Guild.MemberCount))
                    .ToList();

                foreach (var user in reactionUsers.Where(user => !user.IsSelf(_discordClient))
                    .Where(user =>
                        !usersWithMentionRoles.Any(x => x.Value.Roles.Any(y => y.Id == roleId) && x.Key == user.Id)))
                {
                    try
                    {
                        var member = await _textChannel.Guild.GetMemberAsync(user.Id);

                        // Make sure the user is not null, in case they have been banned/left the server
                        await member.GrantRoleAsync(role);
                    }
                    catch (Exception e)
                    {
                        _discordClient.GetCommandsNext().Services.GetRequiredService<ILogger>().Error(e, "Error getting/giving role in VerifyCurrentUserRolesAsync");
                    }
                }

                var userWithRole = usersWithMentionRoles.Where(x => x.Value.Roles.Any(x => x.Id == roleId));
                foreach (var (memberId, member) in userWithRole)
                {
                    if (reactionUsers.Any(x => x.Id == memberId) && !member.IsSelf(_discordClient))
                    {
                        continue;
                    }

                    // User has not reacted, remove the role
                    var guildUser = await _textChannel.Guild.GetMemberAsync(memberId);
                    await guildUser.RevokeRoleAsync(role);
                }
            }
        }

        /// <summary>
        ///     The role should be @Mentioned in the Description of the Embed
        /// </summary>
        private async Task<(bool Result, DiscordRole Role)> TryParseRoleFromEmbedAsync(DiscordMessage input)
        {
            var message = await input.Channel.GetMessageAsync(input.Id);

            if (message.Embeds.Count == 1)
            {
                var embed = message.Embeds.First();
                var guild = _textChannel.Guild;

                // No library provided way to parse the role, the mention should never change, as it uses the ID
                var role = guild.Roles.Values.FirstOrDefault(x =>
                    embed.Description.Contains(x.Mention, StringComparison.InvariantCultureIgnoreCase));
                var result = role != null;
                return (result, role);
            }

            return (false, null);
        }

        private async Task _discordClient_MessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.Guild == null || e.Emoji == null || _textChannel == null || e.Channel.Id != _textChannel.Id ||
                e.Emoji != _config.MentionRoleEmoji)
            {
                return;
            }

            // Check that the message reacted to is a role embed
            if (_existingRoleEmbeds.ContainsValue(e.Message.Id))
            {
                if (e.User is DiscordMember member)
                {
                    // Ignore actions from the bot
                    if (member.IsSelf(_discordClient))
                    {
                        return;
                    }

                    var (result, role) = await TryParseRoleFromEmbedAsync(e.Message);
                    if (result)
                    {
                        await member.GrantRoleAsync(role);
                    }
                }
            }
        }

        private async Task _discordClient_MessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (e.Guild == null && e.Emoji == null || _textChannel == null || e.Channel.Id != _textChannel.Id ||
                e.Emoji != _config.MentionRoleEmoji)
            {
                return;
            }

            // Check that the message reacted to is a role embed
            if (_existingRoleEmbeds.ContainsValue(e.Message.Id))
            {
                if (e.User is DiscordMember member)
                {
                    // Ignore actions from the bot
                    if (member.IsSelf(_discordClient))
                    {
                        return;
                    }

                    var (result, role) = await TryParseRoleFromEmbedAsync(e.Message);
                    if (result)
                    {
                        await member.RevokeRoleAsync(role);
                    }
                }
            }
        }

        public async Task SendRoleEmbed(DiscordRole role)
        {
            // If the role isn't already sent
            if (!_existingRoleEmbeds.ContainsKey(role.Id))
            {
                var embed = new DiscordEmbedBuilder
                {
                    Description = role.Mention,
                    Color = role.Color
                }.Build();

                var message = await _textChannel.SendMessageAsync(embed: embed);
                await message.CreateReactionAsync(_config.MentionRoleEmoji);

                _existingRoleEmbeds.Add(role.Id, message.Id);
            }
        }

        public async Task RemoveRoleEmbed(DiscordRole role)
        {
            // If the role is sent
            if (_existingRoleEmbeds.TryGetValue(role.Id, out var messageId))
            {
                var message = await _textChannel.GetMessageAsync(messageId);
                await message.DeleteAsync();

                _existingRoleEmbeds.Remove(role.Id);
            }
        }
    }
}