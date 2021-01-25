using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Services
{
    [Microservice(MicroserviceType.Manual)]
    public class FaqService
    {
        private readonly Configuration _config;
        private readonly DiscordClient _discordClient;
        private readonly SemaphoreSlim _semaphoreLock = new SemaphoreSlim(1, 1);
        private List<ulong> _faqLockInformed = new List<ulong>();
        private DiscordMessage _lastMessage;
        private DiscordChannel _textChannel;

        public FaqService(DiscordClient discordClient, Configuration config)
        {
            _config = config;

            _discordClient = discordClient;
            _discordClient.GuildDownloadCompleted += _discordClient_GuildsDownloaded;
            _discordClient.MessageReactionAdded += _discordClient_MessageReactionAdded;
        }

        public bool IsEnabled { get; private set; } = true;
        public DiscordMessage FaqMessage => _lastMessage;
        public void Lock()
        {
            IsEnabled = false;
        }

        public async Task UnlockAsync()
        {
            IsEnabled = true;
            await HookToLastMessageAsync();
            await AddUnhandedReactionRolesAsync();
            _faqLockInformed = new List<ulong>();
        }

        private async Task RemoveAllReactionsAsync(DiscordChannel textChannel)
        {
            if (textChannel != null)
            {
                var messages = (await textChannel.GetMessagesAsync()).ToList();

                _lastMessage = messages.OrderByDescending(x => x.Timestamp.Ticks).FirstOrDefault();

                // Remove all existing reactions
                foreach (var message in messages)
                {
                    // If the previous message hooked to, is the same as what is the new last message, don't remove all reactions
                    // This is to prevent unhandled reactions being wiped
                    if (!message.IsUserMessage() || _lastMessage != null && message.Id == _lastMessage.Id)
                    {
                        continue;
                    }

                    if (message.Reactions.Count > 0)
                    {
                        await message.DeleteAllReactionsAsync();
                    }
                }
            }
        }

        public async Task HookToLastMessageAsync()
        {
            await _semaphoreLock.WaitAsync();

            var channel = _discordClient.FindChannel(_config.FaqChannelId);
            if (channel.Type == ChannelType.Text)
            {
                if (_textChannel != null && channel.Id != _textChannel.Id)
                {
                    // If there is a message hooked before, make sure to remove the reaction
                    await RemoveAllReactionsAsync(_textChannel);
                }

                _textChannel = channel;

                await RemoveAllReactionsAsync(_textChannel);

                if (_lastMessage.IsUserMessage())
                {
                    try
                    {
                        await _lastMessage.CreateReactionAsync(_config.FaqRoleEmoji);
                    }
                    catch (Exception e)
                    {
                        // TODO: Rethink this
                        Console.WriteLine(e);
                        _semaphoreLock.Release();
                        throw;
                    }
                }
            }

            _semaphoreLock.Release();
        }

        private Task _discordClient_GuildsDownloaded(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await HookToLastMessageAsync();
                await AddUnhandedReactionRolesAsync();
            });

            return Task.CompletedTask;
        }

        private Task _discordClient_MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await _semaphoreLock.WaitAsync();
                _semaphoreLock.Release();

                if (e.Channel.Id != _textChannel.Id || e.Emoji != _config.FaqRoleEmoji ||
                    !(e.User is DiscordMember member))
                {
                    return;
                }

                if (!IsEnabled)
                {
                    if (_faqLockInformed.Contains(member.Id) || member.Roles.Any(x => x.Id == _config.FaqRoleId))
                    {
                        return;
                    }

                    // Only send the DM once.
                    _faqLockInformed.Add(member.Id);

                    await member.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description =
                            "The FAQ verification process is temporarily locked, this is most likely due to spam bots joining. Please try again later.",
                        Color = DiscordColor.Orange
                    }.Build());

                    return;
                }

                // Check that the message reacted to is the last message in the channel
                if (_lastMessage.Id == e.Message.Id)
                {
                    // Ignore actions from the bot, or if the user already has the role
                    if (!member.IsSelf(_discordClient))
                    {
                        if (member.Roles.All(x => x.Id != _config.FaqRoleId))
                        {
                            var role = _textChannel.Guild.GetRole(_config.FaqRoleId);
                            await member.GrantRoleAsync(role);
                        }

                        if (_lastMessage.IsUserMessage())
                        {
                            await _lastMessage.DeleteReactionAsync(_config.FaqRoleEmoji, member);
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

        public async Task AddUnhandedReactionRolesAsync()
        {
            await _semaphoreLock.WaitAsync();

            // Get all user reactions with the FAQ emoji, max users is the guild member count
            var userReactions =
                (await _lastMessage.GetReactionsAsync(_config.FaqRoleEmoji, _textChannel.Guild.MemberCount))
                .Where(x => !x.IsSelf(_discordClient));

            var role = _textChannel.Guild.GetRole(_config.FaqRoleId);

            foreach (var unhandledUserReaction in userReactions)
            {
                var member = await _textChannel.Guild.GetMemberAsync(unhandledUserReaction.Id);
                if (member != null && member.Roles.All(x => x.Id != _config.FaqRoleId))
                {
                    await member.GrantRoleAsync(role);
                }

                await _lastMessage.DeleteReactionAsync(_config.FaqRoleEmoji, unhandledUserReaction);
            }

            _semaphoreLock.Release();
        }
    }
}
