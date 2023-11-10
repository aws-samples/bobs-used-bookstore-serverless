namespace Orders.Models
{
    public record AddToShoppingCartDto(string CorrelationId, string BookId, int Quantity);
}