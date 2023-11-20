namespace BookInventory.Models
{
    public class PaginatedResult<T> where T : class
    {
        public List<T>? Books { get; set; } = new List<T>();

        public string? NextPageKey { get; set; } = string.Empty;
    }
}