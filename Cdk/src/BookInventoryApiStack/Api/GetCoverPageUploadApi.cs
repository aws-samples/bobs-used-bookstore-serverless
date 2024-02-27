using Amazon.CDK.AWS.Lambda;
using Constructs;
using SharedConstructs;

namespace BookInventoryApiStack.Api;

public class GetCoverPageUploadApi : Construct
{
    public Function Function { get; }

    public GetCoverPageUploadApi(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            $"GeneratePreSignedURLApi",
            new LambdaFunctionProps("./src/BookInventoryApi/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_GetCoverPageUploadUrl_Generated::GetCoverPageUploadUrl",
                Environment = new Dictionary<string, string>(3)
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.SERVICE_NAME },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.METRICS_NAMESPACE},
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true"},//TODO:Enable LogEvent for debugging in non-production environments
                    { "S3_BUCKET_NAME", props.BucketName}
                },
                IsNativeAot = false //dotnet 6 runtime
            }).Function;
    }
}