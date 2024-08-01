namespace BookInventory.Repository.IntegrationTests;

public class BookInventoryRepositoryTestOptions(string tableName, bool isPostFix) 
    : IBookInventoryRepositoryOptions
{
    public string TableName { get; } = tableName;
    public bool IsPostFix { get; } = isPostFix;
}