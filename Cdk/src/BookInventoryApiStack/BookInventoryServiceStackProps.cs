using Amazon.CDK.AWS.Cognito;

namespace BookInventoryApiStack
{
    public class BookInventoryServiceStackProps
    {
        public string BucketName { get; set; }
        
        public string UserPoolId { get; set; }
        
        public string UserPoolClientId { get; set; }
    }
}