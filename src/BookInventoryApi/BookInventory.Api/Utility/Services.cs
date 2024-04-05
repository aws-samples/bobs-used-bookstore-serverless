using Amazon.Rekognition;
using Amazon.S3;
using BookInventory.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Api.Utility;

internal class Services
{
    private static readonly Lazy<IServiceProvider> LazyInstance = new(Build);

    public static IServiceProvider Provider => LazyInstance.Value;

    public static IServiceProvider Init()
    {
        return LazyInstance.Value;
    }

    private static IServiceProvider Build()
    {
        var services = new ServiceCollection();
        services.AddAWSService<IAmazonRekognition>();
        services.AddAWSService<IAmazonS3>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IImageResizeService, ImageResizeService>();
        return services.BuildServiceProvider();
    }
}