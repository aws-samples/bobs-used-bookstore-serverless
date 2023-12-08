namespace SharedConstructs.EventPipeConstruct;

using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Pipes;

using Constructs;

using SharedConstructs.EventPipeConstruct.EventTarget;

public class EventPipe : Construct
{
    private readonly string _id;
    
    private EventSource.EventSource Source { get; set; }
    private EventTarget.EventTarget Target { get; set; }

    public EventPipe(Construct scope, string id) : base(
        scope,
        id)
    {
        this._id = id;
    }
    
    public EventPipe From(EventSource.EventSource source)
    {
        this.Source = source;

        return this;
    }
    
    public EventPipe To(EventTarget.EventTarget target)
    {
        this.Target = target;

        return this;
    }

    public void Build()
    {
        var pipeRole = new Role(
            this,
            "PipeRole",
            new RoleProps { AssumedBy = new ServicePrincipal("pipes.amazonaws.com") });

        switch (this.Target.GetType().Name)
        {
            case nameof(SnsTarget):
                (this.Target as SnsTarget).Topic.GrantPublish(pipeRole);
                break;
            case nameof(WorkflowTarget):
                (this.Target as WorkflowTarget).Workflow.GrantStartExecution(pipeRole);
                (this.Target as WorkflowTarget).Workflow.GrantStartSyncExecution(pipeRole);
                break;
        }
        
        var pipe = new CfnPipe(
            this,
            $"{this._id}-Pipe",
            new CfnPipeProps
            {
                Name = $"{this._id}Pipe",
                RoleArn = pipeRole.RoleArn,
                Source = this.Source.SourceArn,
                SourceParameters = this.Source.SourceParameters,
                Enrichment = null, // Will be added 
                Target = this.Target.TargetArn,
                TargetParameters = this.Target.TargetParameters
            });
    }
}