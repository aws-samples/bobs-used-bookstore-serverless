namespace BookInventory.Common;

public class ApiWrapper<T>
{
    public ApiWrapper(T data, string message)
    {
        this.Message = message;
        this.Data = data;
    }
    
    public string Message { get; set; }
    
    public T Data { get; set; }
}