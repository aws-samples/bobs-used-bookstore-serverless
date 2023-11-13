using Amazon.DynamoDBv2.DataModel;
using BookInventory.Models.Utility;

namespace BookInventory.Models.Common
{
    [DynamoDBTable("BookInventory")]
    public class BaseEntity
    {
        [DynamoDBHashKey]
        public string PK { get; set; }

        [DynamoDBRangeKey]
        public string SK { get; set; }

        [DynamoDBLocalSecondaryIndexRangeKey]
        public string LSI1 { get; set; }

        public string CreatedBy { get; set; } = "System";

        public string CreatedOn { get; set; } = DateTimeUtils.GetCurrentDateTime();

        public string UpdatedOn { get; set; } = DateTimeUtils.GetCurrentDateTime();
    }
}