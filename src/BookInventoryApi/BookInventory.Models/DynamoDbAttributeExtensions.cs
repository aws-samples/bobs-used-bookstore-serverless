namespace BookInventory.Models;

using Amazon.DynamoDBv2.Model;

public static class DynamoDbAttributeExtensions
{
    public static string AsString(this Dictionary<string, AttributeValue> item, string keyName)
    {
        return item.ContainsKey(keyName) ? item[keyName].S : null;
    }
    
    public static decimal AsDecimal(this Dictionary<string, AttributeValue> item, string keyName)
    {
        return item.ContainsKey(keyName) ? decimal.Parse(item[keyName].N) : -1;
    }
    
    public static int AsInt(this Dictionary<string, AttributeValue> item, string keyName)
    {
        return item.ContainsKey(keyName) ? int.Parse(item[keyName].N) : -1;
    }
}