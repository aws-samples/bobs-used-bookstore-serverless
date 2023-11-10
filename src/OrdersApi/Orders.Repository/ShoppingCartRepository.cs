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