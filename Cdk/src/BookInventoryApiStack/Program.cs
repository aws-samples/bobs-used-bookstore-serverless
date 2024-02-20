
using Amazon.CDK;

using BookInventoryApiStack;

var app = new App();

var bookInventoryServiceStack = new BookInventoryServiceStack(
    app,
    $"BookInventoryServiceStack",
    new BookInventoryServiceStackProps(""));

app.Synth();