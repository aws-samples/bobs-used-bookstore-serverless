using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace BookInventoryApiStack.Database
{
    public class BookInventoryDB : Construct
    {
        public Table Table { get; }

        public BookInventoryDB(Construct scope, string id, BookInventoryServiceStackProps props)
            : base(scope, id)
        {
            Table = new Table(this, "BookInventoryTable", new TableProps
            {
                TableName = "BookInventory",
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "PK", Type = AttributeType.STRING },
                SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "SK", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = Amazon.CDK.RemovalPolicy.DESTROY
            });
        }
    }
}