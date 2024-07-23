namespace BookInventory.Common;

using Amazon.XRay.Recorder.Handlers.AwsSdk;

using Repository;
using Service;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public record SharedServiceOptions(bool SkipAppConfiguration = false, bool SkipRepository = false);

public static class StartupExtensions
{
   public static IServiceCollection AddSharedServices(this IServiceCollection services, SharedServiceOptions? options = null)
    {
        AWSSDKHandler.RegisterXRayForAllServices(); ;
        
        if (options is null)
        {
            options = new SharedServiceOptions();
        }

        if (!options.SkipAppConfiguration)
        {
            services.AddApplicationConfiguration();
        }

        if (!options.SkipRepository)
        {
            services.AddSingleton<IBookInventoryRepository, BookInventoryRepository>();
            services.AddSingleton<IBookInventoryService, BookInventoryService>();
            services.AddSingleton<IBookInventoryRepositoryOptions, BookInventoryRepositoryOptions>();
        }

        return services;
    }

    private static IServiceCollection AddApplicationConfiguration(this IServiceCollection services)
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        
        services.AddSingleton<IConfiguration>(config);

        return services;
    }
}