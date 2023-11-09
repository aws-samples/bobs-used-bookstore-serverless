using Amazon.Util;

namespace Orders.Models.Utility
{
    public static class DateTimeUtils
    {
        public static string GetCurrentDateTime()
        {
            return DateTime.UtcNow.ToString(AWSSDKUtils.ISO8601DateFormat);
        }
    }
}