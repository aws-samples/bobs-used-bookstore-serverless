namespace OrderService.Api;

using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;

using OrderService.Models;
using OrderService.Service;

public class Functions
{
    private readonly IOrderService orderService;

    public Functions(IOrderService orderService)
    {
        this.orderService = orderService;
    }

    [LambdaFunction]
    [RestApi(
        LambdaHttpMethod.Get,
        "/orders/{id}")]
    public async Task<Order> GetOrder(string id)
    {
        return await this.orderService.GetOrderAsync(id);
    }

    [LambdaFunction]
    [RestApi(LambdaHttpMethod.Get, "/orders")]
    public async Task<List<Order>> GetOrders()
    {
        return await this.orderService.GetOrdersAsync();
    }
}
