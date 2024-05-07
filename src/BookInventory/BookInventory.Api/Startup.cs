using Amazon.CognitoIdentityProvider;
using Amazon.Rekognition;
using Amazon.S3;
using BookInventory.Api.Utility;
using BookInventory.Api.Validators;
using BookInventory.Common;
using BookInventory.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Api;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    /// <summary>
    /// Services for Lambda functions can be registered in the services dependency injection container in this method.
    ///
    /// The services can be injected into the Lambda function through the containing type's constructor or as a
    /// parameter in the Lambda function using the FromService attribute. Services injected for the constructor have
    /// the lifetime of the Lambda compute container. Services injected as parameters are created within the scope
    /// of the function invocation.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSharedServices();
        services.AddDynamoDBServices();
        services.AddAWSService<IAmazonS3>();
        services.AddAWSService<IAmazonCognitoIdentityProvider>();
        services.AddScoped<IValidator<CreateBookDto>, CreateBookDtoValidator>();
        services.AddScoped<IValidator<UpdateBookDto>, UpdateBookDtoValidator>();
    }
}