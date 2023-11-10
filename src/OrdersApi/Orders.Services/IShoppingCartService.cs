using Orders.Models;

namespace Orders.Services
{
    public interface IShoppingCartService
    {
        Task<IEnumerable<ShoppingCart>> GetShoppingCartAsync(string correlationId);

        Task AddToShoppingCartAsync(AddToShoppingCartDto dto);
    }
}