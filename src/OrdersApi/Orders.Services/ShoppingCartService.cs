using Orders.Models;
using Orders.Repository;

namespace Orders.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository shoppingCartRepository;

        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository)
        {
            this.shoppingCartRepository = shoppingCartRepository;
        }

        public async Task<IEnumerable<ShoppingCart>> GetShoppingCartAsync(string correlationId)
        {
            return await this.shoppingCartRepository.GetAsync(correlationId);
        }

        public async Task AddToShoppingCartAsync(AddToShoppingCartDto dto)
        {
            var shoppingCart = new ShoppingCart(dto.CorrelationId, dto.BookId, dto.Quantity, true);
            await this.shoppingCartRepository.SaveAsync(shoppingCart);
        }
    }
}