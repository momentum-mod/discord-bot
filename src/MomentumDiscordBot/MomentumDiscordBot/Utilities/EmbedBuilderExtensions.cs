using System.Linq;
using Discord;

namespace MomentumDiscordBot.Utilities
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder AddMessageContent(this EmbedBuilder embedBuilder, IMessage message)
        {
            if (message.Author != null)
            {
                embedBuilder.AddField("User",
                    $"{MentionUtils.MentionUser(message.Author.Id)} ({message.Author} {message.Author.Id})");
            }

            if (message.Channel != null)
            {
                embedBuilder.AddField("Channel", MentionUtils.MentionChannel(message.Channel.Id));
            }

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                embedBuilder.AddField("Message", string.Join(string.Empty, message.Content.Take(1024)));

                if (message.Content.Length > 1024)
                {
                    embedBuilder.AddField("Message Overflow",
                        string.Join(string.Empty, message.Content.Skip(1024)));
                }
            }

            var attachments = message.Attachments.ToList();
            for (var i = 0; i < attachments.Count; i++)
            {
                var attachment = attachments[i];

                embedBuilder.AddField($"Attachment {i + 1}", attachment.Url);
            }

            return embedBuilder;
        }
    }
}
