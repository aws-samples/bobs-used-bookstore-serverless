using Microsoft.AspNetCore.Mvc;
using Orders.Models;
using Orders.Services;

namespace Orders.Api.Controllers;

[Route("[controller]")]
public class ShoppingCartController : ControllerBase
{
    private readonly ILogger<ShoppingCartController> logger;
    private readonly IShoppingCartService shoppingCartService;
    private string correlationId = "4e88de55-2962-4d79-b5f6-72f2ac998ebf";//Temporary correlationId

    public ShoppingCartController(ILogger<ShoppingCartController> logger, IShoppingCartService shoppingCartService)
    {
        this.logger = logger;
        this.shoppingCartService = shoppingCartService;
    }

    [HttpGet("ShoppingCart")]
    public async Task<IEnumerable<ShoppingCart>> GetShoppingCart()
    {
        return await this.shoppingCartService.GetShoppingCartAsync(correlationId);
    }

    [HttpGet("WishList")]
    public async Task<IEnumerable<ShoppingCart>> GetWishList()
    {
        return await this.shoppingCartService.GetWishListAsync(correlationId);
    }

    [HttpPost("ShoppingCart")]
    public async Task<IActionResult> AddToShoppingCart(string bookId)
    {
        var dto = new AddToShoppingCartDto(correlationId, bookId, 1);
        await this.shoppingCartService.AddToShoppingCartAsync(dto);
        return Ok();
    }

    [HttpPost("Wishlist")]
    public async Task<IActionResult> AddToWishlist(string bookId)
    {
        var dto = new AddToWishlistDto(correlationId, bookId);
        await this.shoppingCartService.AddToWishlistAsync(dto);
        return Ok();
    }
}