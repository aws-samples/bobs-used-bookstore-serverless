namespace SharedConstructs.EventTarget;

using Amazon.CDK.AWS.Pipes;
using Amazon.CDK.AWS.SQS;

public class SqsQueueTarget : EventTarget
{
    public IQueue Queue { get; }

    public SqsQueueTarget(IQueue queue)
    {
        this.Queue = queue;
        this.TargetParameters = null;
    }
    
    /// <inheritdoc />
    public override string TargetArn => this.Queue.QueueArn;

    /// <inheritdoc />
    public override CfnPipe.PipeTargetParametersProperty TargetParameters { get; }
}