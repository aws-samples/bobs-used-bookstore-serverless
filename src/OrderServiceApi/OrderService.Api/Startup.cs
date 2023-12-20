namespace OrderService.Api;

using Amazon.Lambda.Annotations;

using Microsoft.Extensions.DependencyInjection;

using OrderService.Service;

[LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOrderService, OrderService>();
    }
}