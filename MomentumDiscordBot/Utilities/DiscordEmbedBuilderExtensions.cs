using System.Linq;
using DSharpPlus.Entities;

namespace MomentumDiscordBot.Utilities
{
    public static class DiscordEmbedBuilderExtensions
    {
        public static DiscordEmbedBuilder AddMessageContent(this DiscordEmbedBuilder embedBuilder,
            DiscordMessage message)
        {
            if (message.Author != null)
            {
                embedBuilder.AddField("User",
                    $"{message.Author.Mention} {message.Author}");
            }

            if (message.Channel != null)
            {
                embedBuilder.AddField("Channel", message.Channel.Mention);
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