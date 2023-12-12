namespace SharedConstructs.EventPipeConstruct.EventTarget;

using Amazon.CDK.AWS.Pipes;
using Amazon.CDK.AWS.StepFunctions;

public class WorkflowTarget : EventTarget
{
    public IStateMachine Workflow { get; }

    public WorkflowTarget(IStateMachine workflow)
    {
        this.Workflow = workflow;
        this.TargetParameters = new CfnPipe.PipeTargetParametersProperty()
        {
            StepFunctionStateMachineParameters = new CfnPipe.PipeTargetStateMachineParametersProperty()
            {
                InvocationType = "FIRE_AND_FORGET"
            }
        };
    }

    /// <inheritdoc />
    public override string TargetArn => this.Workflow.StateMachineArn;

    /// <inheritdoc />
    public override CfnPipe.PipeTargetParametersProperty TargetParameters { get; }
}