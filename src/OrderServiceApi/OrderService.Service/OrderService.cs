namespace OrderService.Service;

using global::OrderService.Models;

public class OrderService : IOrderService
{
    public async Task<Order> GetOrderAsync(string id)
    {
        return await Task.FromResult(
            new Order()
            {
                Id = Guid.NewGuid().ToString(),
                CreatedBy = "Test",
                CreateDate = DateTime.Now
            });
    }

    public async Task<List<Order>> GetOrdersAsync()
    {
        return await Task.FromResult(
            new List<Order>()
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedBy = "Test",
                    CreateDate = DateTime.Now
                }
            });
    }
}