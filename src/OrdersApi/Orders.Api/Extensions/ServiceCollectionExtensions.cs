using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Orders.Repository;
using Orders.Services;

namespace Orders.Api.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        internal static void AddApplicationDependencies(this IServiceCollection services)
        {
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

            services.AddSingleton<IShoppingCartRepository, ShoppingCartRepository>();
            services.AddSingleton<IShoppingCartService, ShoppingCartService>();
        }
    }
}