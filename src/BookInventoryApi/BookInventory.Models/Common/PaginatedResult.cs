namespace BookInventory.Models.Common
{
    public class PaginatedResult<T> where T : class
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        public string? NextPageKey { get; set; } = string.Empty;
    }
}