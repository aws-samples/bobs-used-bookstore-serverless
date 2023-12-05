namespace SharedConstructs.EventSource;

using Amazon.CDK.AWS.Pipes;
using Amazon.CDK.AWS.SNS;

public class SnsSource : EventSource
{
    public ITopic Topic { get; }
    
    /// <summary>
    /// Sns Source
    /// </summary>
    /// <param name="topic"></param>
    public SnsSource(ITopic topic)
    {
        this.Topic = topic;
        this.SourceParameters = null;
    }

    /// <summary>
    /// Sns Arn
    /// </summary>
    public override string SourceArn => this.Topic.TopicArn;
    
    /// <summary>
    /// Source parameters
    /// </summary>
    public override CfnPipe.PipeSourceParametersProperty SourceParameters { get; }
}