namespace BookInventory.Service;

using BookInventory.Models;

public record BookQueryResponse(List<BookDto> Books, string Cursor);