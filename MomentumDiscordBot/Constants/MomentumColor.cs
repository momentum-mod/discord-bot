using DSharpPlus.Entities;

namespace MomentumDiscordBot.Constants
{
    public static class MomentumColor
    {
        public static DiscordColor Gray => new(55, 55, 55);
        public static DiscordColor DarkGray => new(42, 42, 42);
        public static DiscordColor DarkestGray => new(32, 32, 32);

        public static DiscordColor LightGray => new(65, 65, 65);
        public static DiscordColor LighterGray => new(79, 79, 79);
        public static DiscordColor LightererGray => new(95, 95, 95);
        public static DiscordColor LighterererGray => new(130, 130, 130);
        public static DiscordColor LightestGray => new(200, 200, 200);

        public static DiscordColor Red => new(255, 106, 106);
        public static DiscordColor Green => new(153, 255, 153);
        public static DiscordColor Blue => new(24, 150, 211);
        public static DiscordColor GrayBlue => new(76, 139, 180);
    }
}