namespace BookInventory.Models
{
    public class BookDto
    {
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