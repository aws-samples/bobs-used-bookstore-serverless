using BookInventory.Models;

namespace BookInventory.Repository;

public interface IBookInventoryRepository : IBaseRepository<Book>
{
    Task<ListResponse> List(int pageSize = 10, string cursor = null);
}