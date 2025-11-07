#pragma warning disable CA2252
var builder = DistributedApplication.CreateBuilder(args);

// List Book
var listBookFunction = builder.AddAWSLambdaFunction<Projects.BookInventory_Api>(name: "ListBooks",
        lambdaHandler: "BookInventory.Api::BookInventory.Api.Functions_ListBooks_Generated::ListBooks")
    .WithEnvironment("TABLE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_SERVICE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_METRICS_NAMESPACE", "BookInventory");

// Get Book
var getBookFunction = builder.AddAWSLambdaFunction<Projects.BookInventory_Api>(name: "GetBook",
    lambdaHandler: "BookInventory.Api::BookInventory.Api.Functions_GetBook_Generated::GetBook")
    .WithEnvironment("TABLE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_SERVICE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_METRICS_NAMESPACE", "BookInventory");

// Add Book
var addBookFunction = builder.AddAWSLambdaFunction<Projects.BookInventory_Api>(name: "AddBook",
    lambdaHandler: "BookInventory.Api::BookInventory.Api.Functions_AddBook_Generated::AddBook")
    .WithEnvironment("TABLE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_SERVICE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_METRICS_NAMESPACE", "BookInventory");

// Update Book
var updateBookFunction = builder.AddAWSLambdaFunction<Projects.BookInventory_Api>(name: "UpdateBook",
    lambdaHandler: "BookInventory.Api::BookInventory.Api.Functions_UpdateBook_Generated::UpdateBook")
    .WithEnvironment("TABLE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_SERVICE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_METRICS_NAMESPACE", "BookInventory");

// Presigned URL 
var preSignedUrlFunction = builder.AddAWSLambdaFunction<Projects.BookInventory_Api>(name: "PreSignedUrl",
        lambdaHandler: "BookInventory.Api::BookInventory.Api.Functions_GetCoverPageUpload_Generated::GetCoverPageUpload")
    .WithEnvironment("TABLE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_SERVICE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_METRICS_NAMESPACE", "BookInventory");

// Authorizer Lambda
var authorizerFunction = builder.AddAWSLambdaFunction<Projects.BookInventory_Authorization>(name: "Authorizer",
    lambdaHandler:
    "BookInventory.Authorization::BookInventory.Authorization.Functions_BookInventoryAuthorizer_Generated::BookInventoryAuthorizer")
    .WithEnvironment("POWERTOOLS_SERVICE_NAME", "BookInventory")
    .WithEnvironment("POWERTOOLS_METRICS_NAMESPACE", "BookInventory");

builder.Build().Run();