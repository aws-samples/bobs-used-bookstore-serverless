namespace BookInventory.Common;

using System.Text.Json.Serialization;

using Amazon.Lambda.APIGatewayEvents;

using BookInventory.Models;
using BookInventory.Models.Request;

[JsonSerializable(typeof(NewBookRequest))]
[JsonSerializable(typeof(Book))]
[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(ApiWrapper<String>))]
[JsonSerializable(typeof(ApiWrapper<Book>))]
[JsonSerializable(typeof(String))]
[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class CustomSerializationContext : JsonSerializerContext
{
}