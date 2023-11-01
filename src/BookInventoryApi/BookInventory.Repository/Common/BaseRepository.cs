using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace BookInventory.Repository
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        private readonly IDynamoDBContext context;

        public BaseRepository(IDynamoDBContext context)
        {
            this.context = context;
        }

        public async Task<TEntity> GetByPrimaryKeyAsync(object partitionKey)
        {
            return await context.LoadAsync<TEntity>(partitionKey);
        }

        public async Task<TEntity> GetByPrimaryKeyAsync(object partitionKey, object sortKey)
        {
            return await context.LoadAsync<TEntity>(partitionKey, sortKey);
        }

        public async Task<IEnumerable<TEntity>> QueryAsync(object partitionKey)
        {
            return await context.QueryAsync<TEntity>(partitionKey).GetRemainingAsync();
        }

        public async Task<IEnumerable<TEntity>> QueryAsync(QueryOperationConfig queryOperationConfig)
        {
            var search = context.FromQueryAsync<TEntity>(queryOperationConfig);
            var items = new List<TEntity>();
            do
            {
                var nextSet = await search.GetNextSetAsync();
                items.AddRange(nextSet);
            }
            while (!search.IsDone);

            return items;
        }

        public async Task SaveAsync(TEntity entity)
        {
            await context.SaveAsync(entity);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            await context.DeleteAsync(entity);
        }

        public async Task DeleteAsync(object partitionKey)
        {
            await context.DeleteAsync<TEntity>(partitionKey);
        }

        public async Task DeleteAsync(object partitionKey, object sortKey)
        {
            await context.DeleteAsync<TEntity>(partitionKey, sortKey);
        }

        public T ConvertToObject<T>(Dictionary<string, AttributeValue> dynamoDbImage)
        {
            var dynamoDbDocument = Document.FromAttributeMap(dynamoDbImage);
            return context.FromDocument<T>(dynamoDbDocument);
        }
    }
}