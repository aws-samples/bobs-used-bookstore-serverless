using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Common
{
    public static class DynamoDBSetup
    {
        public static IServiceCollection AddDynamoDBServices(this IServiceCollection services)
        {
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>((sp) =>
            {
                IAmazonDynamoDB dynamoDBClient = sp.GetRequiredService<IAmazonDynamoDB>();
                var config = new DynamoDBContextConfig
                {
                    DisableFetchingTableMetadata = true
                };
                return new DynamoDBContext(dynamoDBClient, config);
            });

            return services;
        }
    }
}