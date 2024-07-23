using Amazon.DynamoDBv2.Model;

namespace BookInventory.Repository;

public interface IBookInventoryRepositoryOptions
{
    string TableName { get; }
    bool IsPostFix { get; }
}

public class BookInventoryRepositoryOptions : IBookInventoryRepositoryOptions
{
    private const string TABLE_NAME_VAR_NAME = "TABLE_NAME";
    private const string IS_POSTFIX_VAR_NAME = "IS_POSTFIX";
    
    public string TableName => Environment.GetEnvironmentVariable(TABLE_NAME_VAR_NAME) ?? 
                               throw new InvalidOperationException($"{TABLE_NAME_VAR_NAME} was not defined");

    public bool IsPostFix
    {
        get
        {

            var isPostFixString = Environment.GetEnvironmentVariable(IS_POSTFIX_VAR_NAME);
            if (string.IsNullOrEmpty(isPostFixString) ||
                isPostFixString.Equals("false", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}