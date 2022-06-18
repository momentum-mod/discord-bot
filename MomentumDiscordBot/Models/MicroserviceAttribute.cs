using System;

namespace MomentumDiscordBot.Models
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MicroserviceAttribute : Attribute
    {
        public MicroserviceAttribute(MicroserviceType microserviceType = MicroserviceType.Manual) =>
            Type = microserviceType;

        public MicroserviceType Type { get; }
    }
}