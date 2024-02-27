using Amazon.CDK;

using OrderServiceApiStack;

var app = new App();

var orderServiceApiStack = new OrderServiceApiStack.OrderServiceApiStack(
    app,
    $"OrderServiceApiStack",
    new OrderServiceApiStackProps());

app.Synth();