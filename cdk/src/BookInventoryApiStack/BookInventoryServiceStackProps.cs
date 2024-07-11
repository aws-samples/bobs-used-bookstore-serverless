using Amazon.CDK.AWS.Cognito;

namespace BookInventoryApiStack
{
    public class BookInventoryServiceStackProps
    {
        public BookInventoryServiceStackProps(string postfix)
        {
            this.PostFix = postfix;
        }
        
        public string PostFix { get; set; }
        public string BucketName { get; set; }
        public string PublishBucketName { get; set; }
        public string UserPoolId { get; set; }
        public string UserPoolClientId { get; set; }
        
        public string Table { get; set; }
    }
}