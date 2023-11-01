namespace BookInventory.Common;

using Amazon.Lambda.APIGatewayEvents;
using AWS.Lambda.Powertools.Tracing;
using System.Net;
using System.Text.Json;

public static class ApiGatewayResponseBuilder
{
    public static APIGatewayProxyResponse Build<T>(HttpStatusCode statusCode, T body) where T : class
    {
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)statusCode,
            Body = JsonSerializer.Serialize(new ApiWrapper<T>(body, statusCode.ToString()), typeof(ApiWrapper<T>), CustomSerializationContext.Default),
            Headers = new Dictionary<string, string>(1)
            {
                {"ContentType", "application/json"},
                {"traceparent", Tracing.GetEntity().TraceId}
            }
        };
    }

    public static APIGatewayProxyResponse Build(HttpStatusCode statusCode)
    {
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)statusCode,
            Headers = new Dictionary<string, string>(1)
            {
                {"ContentType", "application/json"},
                {"traceparent", Tracing.GetEntity().TraceId}
            }
        };
    }
}