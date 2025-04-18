namespace BookInventoryApiStack;

public class ApiEndpointConfig
{
    public string Path { get; set; }
    public string Method { get; set; }
    public List<string> AllowedRoles { get; set; }
}