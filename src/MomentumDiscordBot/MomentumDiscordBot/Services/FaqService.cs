using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private SemaphoreSlim _semaphoreLock = new SemaphoreSlim(1, 1);
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
            await HookToLastMessageAsync();
            IsEnabled = true;
        }

        private async Task<List<IMessage>> RemoveAllReactionsAsync(SocketTextChannel textChannel)
        {
            if (textChannel != null)
            {
                var messages = (await textChannel.GetMessagesAsync().FlattenAsync()).ToList();

                _lastMessage = messages.OrderByDescending(x => x.Timestamp.Ticks).FirstOrDefault();

                // Remove all existing reactions
                foreach (var message in messages)
                {
                    if (!(message is IUserMessage userMessage) || _lastMessage != null && message.Id == _lastMessage.Id) continue;

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
            await _semaphoreLock.WaitAsync();

            if (_discordClient.GetChannel(_config.FaqChannelId) is SocketTextChannel textChannel)
            {
                if (_textChannel != null && textChannel.Id != _textChannel.Id)
                {
                    // If there is a message hooked before, make sure to remove the reaction
                    await RemoveAllReactionsAsync(_textChannel);
                }

                _textChannel = textChannel;

                var messages = await RemoveAllReactionsAsync(_textChannel);

                if (_lastMessage is IUserMessage lastUserMessage)
                {
                    await lastUserMessage.AddReactionAsync(_config.MentionRoleEmoji);
                }
            }

            _semaphoreLock.Release();
        }

        private Task _discordClient_Ready()
        {
            _ = Task.Run(async () =>
            {
                await HookToLastMessageAsync();
                await AddUnhandedReactionRolesAsync();
            });

            return Task.CompletedTask;
        }
        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> messageBefore,
            ISocketMessageChannel messageAfter, SocketReaction reaction)
        {
            if (!IsEnabled || reaction.Channel.Id != _textChannel.Id || !reaction.Emote.Equals(_config.MentionRoleEmoji)) return;

            await _semaphoreLock.WaitAsync();
            _semaphoreLock.Release();

            // Check that the message reacted to is the last message in the channel
            if (_lastMessage.Id == reaction.MessageId)
            {
                // Get the user as a SocketGuildContext
                var user = _discordClient.Guilds.First(x => x.Channels.Select(x => x.Id).Contains(messageAfter.Id))
                    .Users.First(x => x.Id == reaction.UserId);

                // Ignore actions from the bot, or if the user already has the role
                if (user.IsSelf(_discordClient) || user.Roles.Any(x => x.Id == _config.FaqRoleId)) return;

                var role = _textChannel.Guild.GetRole(_config.FaqRoleId);
                await user.AddRoleAsync(role);

                if (_lastMessage is IUserMessage userMessage)
                {
                    await userMessage.RemoveReactionAsync(_config.MentionRoleEmoji, user);
                }
            }
        }

        public async Task AddUnhandedReactionRolesAsync()
        {
            await _semaphoreLock.WaitAsync();

            var userReactions = (await _lastMessage.GetReactionUsersAsync(_config.MentionRoleEmoji, _textChannel.Guild.MemberCount).FlattenAsync()).
                Where(x => !x.IsSelf(_discordClient));

            var role = _textChannel.Guild.GetRole(_config.FaqRoleId);
            foreach (var unhandledUserReaction in userReactions)
            {
                var guildUser = _textChannel.Guild.GetUser(unhandledUserReaction.Id);
                if (guildUser != null && guildUser.Roles.All(x => x.Id != _config.FaqRoleId))
                {
                    await guildUser.AddRoleAsync(role);
                }

                await _lastMessage.RemoveReactionAsync(_config.MentionRoleEmoji, unhandledUserReaction);
            }

            _semaphoreLock.Release();
        }
    }
}
