﻿using BookInventory.Models.Common;

namespace BookInventory.Models;

using Amazon.DynamoDBv2.DataModel;

public class Book : BaseEntity
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
        PK = BookInventoryConstants.BOOK;
        SK = Guid.NewGuid().ToString();
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

    public int Quantity { get; set; }
    
    public DateTime LastUpdated { get; private set; }

    [DynamoDBGlobalSecondaryIndexHashKey(IndexNames = new[] { "GSI1" }, AttributeName = "GSI1PK")]
    public string LastUpdatedString {
        get => this.LastUpdated.ToString("yyyyMM");
        set
        {
        }
    }
    
    [DynamoDBGlobalSecondaryIndexRangeKey(IndexNames = new[] { "GSI1" }, AttributeName = "GSI1SK")]
    public string BookId {
        get => this.SK;
        set
        {
        }
    }
}