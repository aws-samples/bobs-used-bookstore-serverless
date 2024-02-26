namespace OrderService.Common;

using Amazon.XRay.Recorder.Handlers.AwsSdk;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OrderService.Service;

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
            services.AddSingleton<IOrderService, OrderService>();
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