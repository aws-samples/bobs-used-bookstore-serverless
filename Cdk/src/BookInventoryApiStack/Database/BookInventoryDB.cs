using Amazon.CDK.AWS.DynamoDB;
using BookInventory.Models;
using BookInventory.Models.Common;
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
                TableName = BookInventoryConstants.TABLE_NAME,
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = nameof(BaseEntity.PK), Type = AttributeType.STRING },
                SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = nameof(BaseEntity.SK), Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = Amazon.CDK.RemovalPolicy.DESTROY
            });
        }
    }
}