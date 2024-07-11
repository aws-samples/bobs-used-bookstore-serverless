﻿
using Amazon.CDK;

using BookInventoryApiStack;

var app = new App();

var postFix = System.Environment.GetEnvironmentVariable("STACK_POSTFIX");

var bookInventoryServiceStack = new BookInventoryServiceStack(
    app,
    $"BookInventoryServiceStack{postFix}",
    new BookInventoryServiceStackProps(postFix));

app.Synth();