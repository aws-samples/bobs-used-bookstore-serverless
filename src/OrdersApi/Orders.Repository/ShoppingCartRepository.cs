using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Orders.Models;

namespace Orders.Repository
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly IDynamoDBContext context;

        public ShoppingCartRepository(IDynamoDBContext context)
        {
            this.context = context;
        }

        public async Task SaveAsync(ShoppingCart entity)
        {
            await this.context.SaveAsync(entity);
        }

        public async Task<ShoppingCart> GetAsync(string correlationId, string shoppingCartItemId)
        {
            return await this.context.LoadAsync<ShoppingCart>(OrdersConstants.CART, $"{correlationId}{OrdersConstants.DELIMITER}{shoppingCartItemId}");
        }

        public async Task<IEnumerable<ShoppingCart>> GetAsync(string correlationId)
        {
            var filter = new QueryFilter();
            filter.AddCondition(nameof(ShoppingCart.PK), QueryOperator.Equal, OrdersConstants.CART);
            filter.AddCondition(nameof(ShoppingCart.SK), QueryOperator.BeginsWith, correlationId);
            filter.AddCondition(nameof(ShoppingCart.WantToBuy), QueryOperator.Equal, 1);
            var queryOperationConfig = new QueryOperationConfig
            {
                Filter = filter
            };
            return await QueryAsync(queryOperationConfig);
        }

        public async Task<IEnumerable<ShoppingCart>> GetWishListAsync(string correlationId)
        {
            var filter = new QueryFilter();
            filter.AddCondition(nameof(ShoppingCart.PK), QueryOperator.Equal, OrdersConstants.WISH);
            filter.AddCondition(nameof(ShoppingCart.SK), QueryOperator.BeginsWith, correlationId);
            filter.AddCondition(nameof(ShoppingCart.WantToBuy), QueryOperator.Equal, 0);
            var queryOperationConfig = new QueryOperationConfig
            {
                Filter = filter
            };
            return await QueryAsync(queryOperationConfig);
        }

        private async Task<IEnumerable<ShoppingCart>> QueryAsync(QueryOperationConfig queryOperationConfig)
        {
            var search = this.context.FromQueryAsync<ShoppingCart>(queryOperationConfig);
            var items = new List<ShoppingCart>();
            do
            {
                var nextSet = await search.GetNextSetAsync();
                items.AddRange(nextSet);
            }
            while (!search.IsDone);

            return items;
        }
    }
}