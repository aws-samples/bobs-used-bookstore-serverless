namespace SharedConstructs.EventSource;

using Amazon.CDK.AWS.Pipes;

public abstract class EventSource
{
    /// <summary>
    /// Arn of the source
    /// </summary>
    public abstract string SourceArn { get; }
    
    /// <summary>
    /// Source parameters
    /// </summary>
    public abstract CfnPipe.PipeSourceParametersProperty SourceParameters { get; }
}