using Amazon.DynamoDBv2.DocumentModel;

namespace BookInventory.Repository
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByPrimaryKeyAsync(object partitionKey);

        Task<TEntity?> GetByPrimaryKeyAsync(object partitionKey, object sortKey);

        Task<IEnumerable<TEntity>> QueryAsync(object partitionKey);

        Task<IEnumerable<TEntity>> QueryAsync(QueryOperationConfig queryOperationConfig);

        Task SaveAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);

        Task DeleteAsync(object partitionKey, object sortKey);
    }
}