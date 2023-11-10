using Orders.Models;

namespace Orders.Repository
{
    public interface IShoppingCartRepository
    {
        Task<IEnumerable<ShoppingCart>> GetAsync(string correlationId);

        Task SaveAsync(ShoppingCart entity);
    }
}