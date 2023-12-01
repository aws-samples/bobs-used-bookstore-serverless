namespace BookInventory.Models
{
    public class BookDto
    {
        public BookDto()
        {
        }

        public BookDto(Book book)
        {
            this.Author = book.Author;
            this.BookId = book.BookId;
            this.BookType = book.BookType;
            this.Condition = book.Condition;
            this.CoverImage = book.CoverImageUrl;
            this.Genre = book.Genre;
            this.ISBN = book.ISBN;
            this.Name = book.Name;
            this.Price = book.Price;
            this.Publisher = book.Publisher;
            this.Quantity = book.Quantity;
            this.Summary = book.Summary;
            this.Year = book.Year;
        }
        
        public string Author { get; set; }
        public string BookId { get; set; }
        public string BookType { get; set; }
        public string Condition { get; set; }
        public string? CoverImage { get; set; }
        public string Genre { get; set; }
        public string ISBN { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Publisher { get; set; }
        public int Quantity { get; set; }
        public string Summary { get; set; }
        public int? Year { get; set; }
    }
}