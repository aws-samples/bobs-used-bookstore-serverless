using Amazon.DynamoDBv2.DataModel;
using Amazon.Util;

namespace BookInventory.Models;

[DynamoDBTable("BookInventory")]
public class Book
{
    public const int LowBookThreshold = 5;

    public Book()
    {
    }

    public Book(
           string name,
           string author,
           string ISBN,
           string publisher,
           string bookType,
           string genre,
           string condition,
           decimal price,
           int quantity,
           string summary,
           int? year = null,
           string? coverImageUrl = null)
    {
        BookId = Guid.NewGuid().ToString();
        Name = name;
        Author = author;
        this.ISBN = ISBN;
        Publisher = publisher;
        BookType = bookType;
        Genre = genre;
        Condition = condition;
        Price = price;
        Quantity = quantity;
        Year = year;
        Summary = summary;
        CoverImageUrl = coverImageUrl;
        LastUpdated = DateTime.Now;
    }

    [DynamoDBHashKey]
    public string BookId { get; set; }

    public string Name { get; set; }

    public string Author { get; set; }

    public int? Year { get; set; }

    public string ISBN { get; set; }

    public string Publisher { get; set; }

    public string BookType { get; set; }

    public string Genre { get; set; }

    public string Condition { get; set; }

    public string? CoverImageUrl { get; set; }

    public string Summary { get; set; }

    public decimal Price { get; set; }
    public string CreatedBy { get; set; } = "System";

    public string CreatedOn { get; set; } = DateTime.UtcNow.ToString(AWSSDKUtils.ISO8601DateFormat);

    public int Quantity { get; set; }

    public DateTime LastUpdated { get; private set; }

    [DynamoDBGlobalSecondaryIndexHashKey(IndexNames = new[] { "GSI1" }, AttributeName = "GSI1PK")]
    public string LastUpdatedString
    {
        get => this.LastUpdated.ToString("yyyyMM");
        set
        {
        }
    }

    [DynamoDBGlobalSecondaryIndexRangeKey(IndexNames = new[] { "GSI1" }, AttributeName = "GSI1SK")]
    public string GSI1SK
    {
        get => this.BookId;
        set
        {
        }
    }
}