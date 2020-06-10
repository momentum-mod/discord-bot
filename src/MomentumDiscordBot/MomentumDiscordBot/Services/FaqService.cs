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
            await HookToLastMessageAsync();
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
    }
}
