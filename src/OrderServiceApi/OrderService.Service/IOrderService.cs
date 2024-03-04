namespace OrderService.Service;

using global::OrderService.Models;

public interface IOrderService
{
    Task<Order> GetOrderAsync(string id);
    
    Task<List<Order>> GetOrdersAsync();
}