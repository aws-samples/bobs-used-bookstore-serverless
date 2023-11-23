namespace BookInventory.Common;

using Amazon.Lambda.APIGatewayEvents;
using BookInventory.Models;
using BookInventory.Models.Common;
using System.Text.Json.Serialization;

using BookInventory.Service;

[JsonSerializable(typeof(CreateBookDto))]
[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(ApiWrapper<string>))]
[JsonSerializable(typeof(ApiWrapper<BookDto>))]
[JsonSerializable(typeof(ApiWrapper<List<BookDto>>))]
[JsonSerializable(typeof(ApiWrapper<BookQueryResponse>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class CustomSerializationContext : JsonSerializerContext
{
}