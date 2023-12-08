namespace SharedConstructs.EventPipeConstruct.EventSource;

using Amazon.CDK.AWS.Pipes;
using Amazon.CDK.AWS.SQS;

public class SqsQueueSource : EventSource
{
    public IQueue Queue { get; }

    public SqsQueueSource(IQueue queue)
    {
        this.Queue = queue;
        this.SourceParameters = new CfnPipe.PipeSourceParametersProperty
        {
            SqsQueueParameters = new CfnPipe.PipeSourceSqsQueueParametersProperty()
            {
                BatchSize = 1
            }
        };
    }
    
    /// <inheritdoc />
    public override string SourceArn => this.Queue.QueueArn;

    /// <inheritdoc />
    public override CfnPipe.PipeSourceParametersProperty SourceParameters { get; }
}