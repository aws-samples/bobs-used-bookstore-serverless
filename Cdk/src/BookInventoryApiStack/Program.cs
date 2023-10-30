
using Amazon.CDK;

using BookInventoryApiStack;

var app = new App();

var stockPriceStack = new BookInventoryServiceStack(
    app,
    $"BookInventoryServiceStack",
    new BookInventoryServiceStackProps());

app.Synth();