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
            $"GetCoverPageUploadApi",
            new LambdaFunctionProps("./src/BookInventory/BookInventory.Api")
            {
                Handler = "BookInventory.Api::BookInventory.Api.Functions_GetCoverPageUpload_Generated::GetCoverPageUpload",
                Environment = new Dictionary<string, string>(5)
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.SERVICE_NAME },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.METRICS_NAMESPACE},
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true"},//TODO:Enable LogEvent for debugging in non-production environments
                    { "S3_BUCKET_NAME", props.BucketName },
                    { "EXPIRY_DURATION", "5" },
                },
                IsNativeAot = false //dotnet 8 runtime
            }).Function;
    }
}