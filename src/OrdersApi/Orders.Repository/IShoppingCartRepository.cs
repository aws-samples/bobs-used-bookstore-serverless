using Orders.Models;

namespace Orders.Repository
{
    public interface IShoppingCartRepository
    {
        Task<IEnumerable<ShoppingCart>> GetAsync(string correlationId);

        Task<IEnumerable<ShoppingCart>> GetWishListAsync(string correlationId);

        Task<ShoppingCart> GetAsync(string correlationId, string shoppingCartItemId);

        Task SaveAsync(ShoppingCart entity);
    }
}