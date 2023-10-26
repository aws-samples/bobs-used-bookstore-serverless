namespace BookInventory.Repository;

using BookInventory.Models;

public class BookInventoryRepository : IBookInventoryRepository
{
    private IList<Book> books = new List<Book>()
    {
        new()
        {
            Id = "100",
            Name = "History Book"
        },
        new()
        {
            Id = "200",
            Name = "Science Book"
        },
        new()
        {
            Id = "300",
            Name = "Literature Book"
        },
        new()
        {
            Id = "400",
            Name = "Kids Book"
        }
    };

    public async Task<IList<Book>> GetAllBooks()
    {
        return await Task.FromResult(this.books);
    }

    public async Task<Book> GetBookById(string id)
    {
        return await Task.FromResult(this.books.First(x => x.Id.Equals(id)));
    }

    public async Task<int> AddBook(Book newBook)
    {
        this.books.Add(newBook);
        return await Task.FromResult(1);
    }
}