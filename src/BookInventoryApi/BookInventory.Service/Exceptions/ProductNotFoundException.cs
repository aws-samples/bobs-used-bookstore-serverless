namespace BookInventory.Service.Exceptions
{
    public class ProductNotFoundException : Exception
    {
        public string ProductId { get; set; }

        public ProductNotFoundException()
        {
        }

        public ProductNotFoundException(string message) : base(message)
        {
        }

        public ProductNotFoundException(string message, string productId) : base(message)
        {
            ProductId = productId;
        }
    }
}