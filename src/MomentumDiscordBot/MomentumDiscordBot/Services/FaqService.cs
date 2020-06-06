using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Services
{
    public class FaqService
    {
        private readonly Config _config;
        private readonly DiscordSocketClient _discordClient;
        private SocketTextChannel _textChannel;
        private IMessage _lastMessage;
        public bool IsEnabled { get; private set; } = true;
        public FaqService(DiscordSocketClient discordClient, Config config)
        {
            _config = config;

            _discordClient = discordClient;
            _discordClient.Ready += _discordClient_Ready;
            _discordClient.ReactionAdded += ReactionAdded;
        }

        public void Lock()
        {
            IsEnabled = false;
        }

        public async Task UnlockAsync()
        {
            await RemoveAllReactionsAsync(_textChannel);
            IsEnabled = true;
        }

        private async Task<List<IMessage>> RemoveAllReactionsAsync(SocketTextChannel textChannel)
        {
            if (textChannel != null)
            {
                var messages = (await textChannel.GetMessagesAsync().FlattenAsync()).ToList();

                // Remove all existing reactions
                foreach (var message in messages)
                {
                    if (!(message is IUserMessage userMessage)) continue;

                    if (userMessage.Reactions.Count > 0)
                    {
                        await userMessage.RemoveAllReactionsAsync();
                    }
                }

                return messages;
            }

            return null;
        }

        public async Task HookToLastMessageAsync()
        {
            // If there is a message hooked before, make sure to remove the reaction
            await RemoveAllReactionsAsync(_textChannel);

            _textChannel = _discordClient.GetChannel(_config.FaqChannelId) as SocketTextChannel;

            if (_textChannel != null)
            {
                var messages = await RemoveAllReactionsAsync(_textChannel);

                _lastMessage = messages.OrderByDescending(x => x.Timestamp.Ticks).FirstOrDefault();

                if (_lastMessage is IUserMessage lastUserMessage)
                {
                    await lastUserMessage.AddReactionAsync(_config.MentionRoleEmoji);
                }
            }
        }

        private Task _discordClient_Ready()
        {
            _ = Task.Run(async () =>
            {
                await HookToLastMessageAsync();
                await VerifyCurrentUserRolesAsync();
            });

            return Task.CompletedTask;
        }
        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> messageBefore,
            ISocketMessageChannel messageAfter, SocketReaction reaction)
        {
            if (!IsEnabled || reaction.Channel.Id != _textChannel.Id || !reaction.Emote.Equals(_config.MentionRoleEmoji)) return;

            // Check that the message reacted to is the last message in the channel
            if (_lastMessage.Id == reaction.MessageId)
            {
                // Get the user as a SocketGuildContext
                var user = _discordClient.Guilds.First(x => x.Channels.Select(x => x.Id).Contains(messageAfter.Id))
                    .Users.First(x => x.Id == reaction.UserId);

                // Ignore actions from the bot
                if (user.IsSelf(_discordClient)) return;

                var role = _textChannel.Guild.GetRole(_config.FaqRoleId);
                await user.AddRoleAsync(role);

                if (_lastMessage is IUserMessage userMessage)
                {
                    await userMessage.RemoveReactionAsync(_config.MentionRoleEmoji, user);
                }
            }
        }

        private async Task VerifyCurrentUserRolesAsync()
        {
            var usersWithMentionRoles = _textChannel.Guild.Users.Where(x => _config.MentionRoles.Intersect(x.Roles.Select(y => y.Id)).Any()).ToList();

            // Check users who have reacted to last message
            var role = _textChannel.Guild.GetRole(_config.FaqRoleId);

            if (!(_lastMessage is IUserMessage userMessage)) return;

            // Get all users who have reacted to the embed
            var reactionUsers = (await userMessage.GetReactionUsersAsync(_config.MentionRoleEmoji, _textChannel.Guild.MemberCount).FlattenAsync()).ToList();

            foreach (var guildUser in reactionUsers.Where(user => !user.IsSelf(_discordClient))
                .Where(user =>
                    !usersWithMentionRoles.Any(x => x.Roles.Any(y => y.Id == _config.FaqRoleId) && x.Id == user.Id))
                .Select(user => _textChannel.Guild.GetUser(user.Id)))
            {
                // Make sure the user is not null, in case they have been banned/left the server
                if (guildUser != null)
                {
                    await guildUser.AddRoleAsync(role);
                }
            }

            var userWithRole = usersWithMentionRoles.Where(x => x.Roles.Any(x => x.Id == _config.FaqRoleId));
            foreach (var user in userWithRole)
            {
                if (reactionUsers.Any(x => x.Id == user.Id) && !user.IsSelf(_discordClient)) continue;

                // User has not reacted, remove the role
                var guildUser = _textChannel.Guild.GetUser(user.Id);
                await guildUser.RemoveRoleAsync(role);
            }
        }
    }
}
