using Amazon.DynamoDBv2.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookInventory.Repository.Extensions
{
    public static class DynamoDBExtensions
    {
        public static string? SerializePageKey(this Dictionary<string, AttributeValue> pageKey)
        {
            return pageKey.Count == 0 ? null :
                Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(pageKey, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                }));
        }

        public static Dictionary<string, AttributeValue> DeserializePageKey(this string pageKey)
        {
            return JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(Convert.FromBase64String(pageKey));
        }
    }
}