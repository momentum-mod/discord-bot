using System;

namespace MomentumDiscordBot.Models
{
    public class MicroserviceAttribute : Attribute
    {
        public MicroserviceAttribute(MicroserviceType microserviceType = MicroserviceType.Manual) =>
            Type = microserviceType;

        public MicroserviceType Type { get; }
    }
}