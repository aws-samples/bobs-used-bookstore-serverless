using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using DotNet.Testcontainers.Containers;
using Testcontainers.DynamoDb;

namespace BookInventory.Repository.IntegrationTests.Fixtures;

public class DynamoDbFixture : IDisposable
{
    private readonly IContainer dynamoDbContainer;
    public IDynamoDBContext DynamoDbContext { get; }
    public IAmazonDynamoDB DynamoDbClient { get; }

    public DynamoDbFixture()
    {
        dynamoDbContainer = new DynamoDbBuilder()
            .WithPortBinding(port: 8000, assignRandomHostPort: true)
            .Build();

        dynamoDbContainer.StartAsync().Wait();

        var serviceUrl = $"http://localhost:{this.dynamoDbContainer.GetMappedPublicPort(8000)}";

        DynamoDbClient = new AmazonDynamoDBClient(
            //The DynamoDB SDK requires credentials, but these aren't used.
            new BasicAWSCredentials("test", "test"),
            new AmazonDynamoDBConfig
            {
                // Override the ServiceURL using the local version. 
                // IMPORTANT! If you set the RegionEndpoint here that will override any value you set in the ServiceURL.
                ServiceURL = serviceUrl,
            });

       DynamoDbContext = new DynamoDBContext(DynamoDbClient, new DynamoDBContextConfig
        {
            DisableFetchingTableMetadata = true
        });
    }
    
    public void Dispose()
    {
        dynamoDbContainer.DisposeAsync().GetAwaiter().GetResult();
        DynamoDbContext.Dispose();
        DynamoDbClient.Dispose();
    }
}