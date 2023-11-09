namespace Orders.Models
{
    public record AddToShoppingCartDto(string CorrelationId, string BookId, int Quantity);

    public record AddToWishlistDto(string CorrelationId, string BookId);
}