using Orders.Models;

namespace Orders.Services
{
    public interface IShoppingCartService
    {
        Task<IEnumerable<ShoppingCart>> GetWishListAsync(string correlationId);

        Task<IEnumerable<ShoppingCart>> GetShoppingCartAsync(string correlationId);

        Task AddToShoppingCartAsync(AddToShoppingCartDto dto);

        Task AddToWishlistAsync(AddToWishlistDto dto);
    }
}