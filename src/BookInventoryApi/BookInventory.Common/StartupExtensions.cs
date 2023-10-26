namespace BookInventory.Common;

using Amazon.XRay.Recorder.Handlers.AwsSdk;

using BookInventory.Repository;
using BookInventory.Service;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public record SharedServiceOptions(bool SkipAppConfiguration = false, bool SkipRepository = false);

public static class StartupExtensions
{
   public static IServiceCollection AddSharedServices(this IServiceCollection services, SharedServiceOptions? options = null)
    {
        AWSSDKHandler.RegisterXRayForAllServices();
        
        var postfix = Environment.GetEnvironmentVariable("STACK_POSTFIX");
        
        if (options is null)
        {
            options = new SharedServiceOptions();
        }

        if (!options.SkipAppConfiguration)
        {
            services.AddApplicationConfiguration(postfix);
        }

        if (!options.SkipRepository)
        {
            services.AddSingleton<IBookInventoryRepository, BookInventoryRepository>();
            services.AddSingleton<IBookInventoryService, BookInventoryService>();
        }

        return services;
    }

    private static IServiceCollection AddApplicationConfiguration(this IServiceCollection services, string postfix)
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        
        services.AddSingleton<IConfiguration>(config);

        return services;
    }
}