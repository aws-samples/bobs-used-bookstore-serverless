namespace SharedConstructs.EventPipeConstruct.EventTarget;

using Amazon.CDK.AWS.Pipes;
using Amazon.CDK.AWS.SNS;

public class SnsTarget : EventTarget
{
    public ITopic Topic { get; }

    public SnsTarget(ITopic topic)
    {
        this.Topic = topic;
        this.TargetParameters = null;
    }

    /// <inheritdoc />
    public override string TargetArn => this.Topic.TopicArn;

    /// <inheritdoc />
    public override CfnPipe.PipeTargetParametersProperty TargetParameters { get; }
}