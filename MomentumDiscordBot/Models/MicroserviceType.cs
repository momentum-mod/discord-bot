namespace MomentumDiscordBot.Models
{
    public enum MicroserviceType
    {
        /// <summary>
        ///     Does nothing with the service automatically
        /// </summary>
        Manual = 0,

        /// <summary>
        ///     Adds the service to the DI provider
        /// </summary>
        Inject = 1,

        /// <summary>
        ///     Initializes the services through `IServiceProvider#GetRequiredService()`
        /// </summary>
        InjectAndInitialize = 1 << 1
    }
}