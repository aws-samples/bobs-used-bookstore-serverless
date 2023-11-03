using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using BookInventory.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Common
{
    public static class DynamoDBSetup
    {
        public static IServiceCollection AddDynamoDBServices(this IServiceCollection services)
        {
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

            return services;
        }
    }
}