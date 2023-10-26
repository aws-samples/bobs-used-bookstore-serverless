using Amazon.CDK;

using AuthenticationStack;

var app = new App();

var postFix = System.Environment.GetEnvironmentVariable("STACK_POSTFIX");

var authenticationStack = new AuthenticationStack.AuthenticationStack(
    app,
    $"AuthenticationStack{postFix}",
    new AuthenticationProps($"{postFix}"));

app.Synth();