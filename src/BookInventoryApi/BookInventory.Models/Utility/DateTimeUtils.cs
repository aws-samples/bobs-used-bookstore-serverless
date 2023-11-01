using Amazon.Util;

namespace BookInventory.Models.Utility
{
    public static class DateTimeUtils
    {
        public static string GetCurrentDateTime()
        {
            return DateTime.UtcNow.ToString(AWSSDKUtils.ISO8601DateFormat);
        }
    }
}