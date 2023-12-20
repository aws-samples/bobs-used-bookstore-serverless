namespace OrderService.Models;

public class Order
{
    public string Id { get; set; }
    public DateTime CreateDate { get; set; }
    public string CreatedBy { get; set; }
}