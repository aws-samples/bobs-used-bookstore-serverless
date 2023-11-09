using Amazon.DynamoDBv2.DataModel;
using Amazon.Util;
using Orders.Models.Utility;

namespace Orders.Models.Common
{
    [DynamoDBTable("Orders")]
    public class BaseEntity
    {
        [DynamoDBHashKey]
        public string PK { get; set; }

        [DynamoDBRangeKey]
        public string SK { get; set; }

        public string CreatedBy { get; set; } = "System";

        public string CreatedOn { get; set; } = DateTimeUtils.GetCurrentDateTime();

        public string UpdatedOn { get; set; } = DateTimeUtils.GetCurrentDateTime();
    }
}