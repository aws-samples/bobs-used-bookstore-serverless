using Amazon.CDK.AWS.Lambda;
using Constructs;
using SharedConstructs;

namespace BookInventoryApiStack.ImageValidation;

public class ResizeImage  : Construct
{
    public Function Function { get; }

    public ResizeImage(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            Constants.RESIZE_IMAGE,
            new LambdaFunctionProps("./src/BookInventory/BookInventory.ImageWorkflow")
            {
                Handler = "BookInventory.ImageWorkflow::BookInventory.ImageWorkflow.Functions_SaveResizedImage_Generated::SaveResizedImage",
                Environment = new Dictionary<string, string>
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.RESIZE_IMAGE },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.RESIZE_IMAGE },
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true" },
                    {"DESTINATION_BUCKET",props.PublishBucketName}
                }
            }).Function;
    }
}