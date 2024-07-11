namespace SharedConstructs.EventPipeConstruct.EventSource;

using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Pipes;

public class DynamoDbSource : EventSource
{
    public ITable Table { get; }

    public DynamoDbSource(ITable table)
    {
        this.Table = table;
        this.SourceParameters = new CfnPipe.PipeSourceParametersProperty
        {
            DynamoDbStreamParameters = new CfnPipe.PipeSourceDynamoDBStreamParametersProperty
            {
                StartingPosition = "LATEST",
                BatchSize = 1
            }
        };
    }
    
    /// <summary>
    /// Sns Arn
    /// </summary>
    public override string SourceArn => this.Table.TableArn;
    
    /// <summary>
    /// Source parameters
    /// </summary>
    public override CfnPipe.PipeSourceParametersProperty SourceParameters { get; }
}