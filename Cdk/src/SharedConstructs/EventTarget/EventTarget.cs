namespace SharedConstructs.EventTarget;

using Amazon.CDK.AWS.Pipes;

public abstract class EventTarget
{
    /// <summary>
    /// Arn of the target
    /// </summary>
    public abstract string TargetArn { get; }
    
    /// <summary>
    /// Parameters of the target
    /// </summary>
    public abstract CfnPipe.PipeTargetParametersProperty TargetParameters { get; }
}