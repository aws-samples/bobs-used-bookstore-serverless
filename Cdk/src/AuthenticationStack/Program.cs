using Amazon.CDK;

using AuthenticationStack;

var app = new App();

var authenticationStack = new AuthenticationStack.AuthenticationStack(
    app,
    $"AuthenticationStack",
    new AuthenticationProps());

app.Synth();