namespace OrderService.Common;

using System.Text.Json.Serialization;

[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class CustomSerializationContext : JsonSerializerContext
{
}