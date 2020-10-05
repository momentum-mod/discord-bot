using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Utilities
{
    public static class MicroserviceExtensions
    {
        public static IServiceCollection InjectMicroservices(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.ExportedTypes.Where(type =>
            {
                var typeInfo = type.GetTypeInfo();

                // Does it have the `MicroserviceAttribute` 
                return typeInfo.GetCustomAttributes().Any(x => x.GetType() == typeof(MicroserviceAttribute));
            }).ToList();

            foreach (var type in types)
            {
                var microserviceAttribute = type.GetCustomAttribute<MicroserviceAttribute>();

                if (microserviceAttribute != null && (microserviceAttribute.Type == MicroserviceType.Inject ||
                                                      microserviceAttribute.Type ==
                                                      MicroserviceType.InjectAndInitialize))
                {
                    services.AddSingleton(type);
                }
            }

            return services;
        }
}