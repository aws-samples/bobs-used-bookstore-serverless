using Amazon.CDK;

using OrderServiceApiStack;

var app = new App();
var postFix = System.Environment.GetEnvironmentVariable("STACK_POSTFIX");
var orderServiceApiStack = new OrderServiceApiStack.OrderServiceApiStack(
    app,
    $"OrderServiceApiStack{postFix}",
    new OrderServiceApiStackProps(postFix));

app.Synth();