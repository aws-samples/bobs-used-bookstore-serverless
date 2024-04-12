using Amazon.Lambda.SQSEvents;
using AWS.Lambda.Powertools.BatchProcessing;

namespace BookInventory.Common;

using Amazon.Lambda.APIGatewayEvents;
using BookInventory.Models;
using BookInventory.Service;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(CreateBookDto))]
[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(ApiWrapper<string>))]
[JsonSerializable(typeof(ApiWrapper<BookDto>))]
[JsonSerializable(typeof(ApiWrapper<List<BookDto>>))]
[JsonSerializable(typeof(ApiWrapper<BookQueryResponse>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(SQSEvent))]
[JsonSerializable(typeof(BatchItemFailuresResponse))]
[JsonSerializable(typeof(APIGatewayCustomAuthorizerRequest))]
[JsonSerializable(typeof(APIGatewayCustomAuthorizerResponse))]
public partial class CustomSerializationContext : JsonSerializerContext
{
}