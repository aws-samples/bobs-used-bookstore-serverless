using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace SharedConstructs
{
    public class DynamoDBTable : Construct
    {
        public Table Table { get; }

        public DynamoDBTable(Construct scope, string id, TableProps props)
            : base(scope, id)
        {
            Table = new Table(this, id, new TableProps
            {
                TableName = props.TableName ?? id,
                PartitionKey = props.PartitionKey ?? new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "PK", Type = AttributeType.STRING },
                SortKey = props.SortKey ?? new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "SK", Type = AttributeType.STRING },
                BillingMode = props.BillingMode ?? BillingMode.PAY_PER_REQUEST
            });
        }
    }
}