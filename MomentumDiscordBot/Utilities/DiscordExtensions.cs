using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;

namespace MomentumDiscordBot.Utilities
{
    public static class DiscordExtensions
    {
        private const Permissions DangerousGuildPermissions = Permissions.Administrator | Permissions.BanMembers |
                                                              Permissions.DeafenMembers | Permissions.KickMembers |
                                                              Permissions.ManageChannels | Permissions.ManageEmojis |
                                                              Permissions.ManageGuild | Permissions.ManageMessages |
                                                              Permissions.ManageNicknames | Permissions.ManageRoles |
                                                              Permissions.ManageWebhooks | Permissions.MoveMembers |
                                                              Permissions.MuteMembers | Permissions.ViewAuditLog |
                                                              Permissions.UseExternalEmojis;

        public static IEnumerable<DiscordMessage> FromSelf(this IEnumerable<DiscordMessage> source,
            DiscordClient discordClient)
            => source.Where(x => x.Author.Id == discordClient.CurrentUser.Id);

        public static bool IsSelf(this DiscordUser user, DiscordClient discordClient)
            => discordClient.CurrentUser.Id == user.Id;

        public static bool RequireRole(this DiscordUser user, ulong roleId)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            if (!(user is DiscordMember member))
            {
                return false;
            }

            return member.Roles.Any(role => role.Id == roleId);
        }

        public static Permissions GetDangerousPermissions(this Permissions guildPermissions)
            => DangerousGuildPermissions & guildPermissions;

        public static bool IsUserMessage(this DiscordMessage message)
            => message.Author != null &&
               !message.Author.IsBot &&
               (!message.Author.IsSystem ?? true) &&
               message.MessageType.HasValue &&
               message.MessageType == MessageType.Default;

        public static DiscordChannel FindChannel(this DiscordClient discordClient, ulong id)
            => discordClient.Guilds
                .SelectMany(x => x.Value.Channels.Values)
                .FirstOrDefault(x => x != null && x.Id == id);

        public static DiscordRole FindRole(this DiscordClient discordClient, ulong id)
            => discordClient.Guilds
                .SelectMany(x => x.Value.Roles.Values)
                .FirstOrDefault(x => x != null && x.Id == id);
    }
}