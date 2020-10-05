using DSharpPlus.Entities;

namespace MomentumDiscordBot.Constants
{
    public static class MomentumColor
    {
        public static DiscordColor Gray => new DiscordColor(55, 55, 55);
        public static DiscordColor DarkGray => new DiscordColor(42, 42, 42);
        public static DiscordColor DarkestGray => new DiscordColor(32, 32, 32);

        public static DiscordColor LightGray => new DiscordColor(65, 65, 65);
        public static DiscordColor LighterGray => new DiscordColor(79, 79, 79);
        public static DiscordColor LightererGray => new DiscordColor(95, 95, 95);
        public static DiscordColor LighterererGray => new DiscordColor(130, 130, 130);
        public static DiscordColor LightestGray => new DiscordColor(200, 200, 200);

        public static DiscordColor Red => new DiscordColor(255, 106, 106);
        public static DiscordColor Green => new DiscordColor(153, 255, 153);
        public static DiscordColor Blue => new DiscordColor(24, 150, 211);
        public static DiscordColor GrayBlue => new DiscordColor(76, 139, 180);
    }
}