using Amazon.CDK.AWS.Lambda;
using Constructs;
using SharedConstructs;

namespace BookInventoryApiStack.ImageValidation;

public class ValidateImage : Construct
{
    public Function Function { get; }

    public ValidateImage(Construct scope, string id, BookInventoryServiceStackProps props) : base(
        scope,
        id)
    {
        this.Function = new LambdaFunction(
            this,
            Constants.VALIDATE_IMAGE,
            new LambdaFunctionProps("./src/BookInventory/BookInventory.ImageWorkflow")
            {
                Handler = "BookInventory.ImageWorkflow::BookInventory.ImageWorkflow.Functions_ImageValidation_Generated::ImageValidation",
                Environment = new Dictionary<string, string>
                {
                    { "POWERTOOLS_SERVICE_NAME", Constants.VALIDATE_IMAGE },
                    { "POWERTOOLS_METRICS_NAMESPACE", Constants.VALIDATE_IMAGE },
                    { "POWERTOOLS_LOGGER_LOG_EVENT", "true" }
                }
            }).Function;
    }
}