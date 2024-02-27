
using Amazon.CDK;

using BookInventoryApiStack;

var app = new App();

var inventoryServiceStack = new BookInventoryServiceStack(
    app,
    $"BookInventoryServiceStack",
    new BookInventoryServiceStackProps());

app.Synth();