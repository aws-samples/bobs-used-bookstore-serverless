using Microsoft.VisualBasic;
using Orders.Models.Common;

namespace Orders.Models
{
    public class ShoppingCart : BaseEntity
    {
        public ShoppingCart()
        { }

        public ShoppingCart(string correlationId, string bookId, int quantity, bool wantToBuy)
        {
            PK = OrdersConstants.CART;
            SK = $"{correlationId}{OrdersConstants.DELIMITER}{Guid.NewGuid().ToString()}";
            CorrelationId = correlationId;
            BookId = bookId;
            Quantity = quantity;
            WantToBuy = wantToBuy;
        }

        public ShoppingCart(string correlationId, string bookId, bool wantToBuy)
        {
            PK = OrdersConstants.CART;
            SK = $"{correlationId}{OrdersConstants.DELIMITER}{Guid.NewGuid().ToString()}";
            CorrelationId = correlationId;
            BookId = bookId;
            WantToBuy = wantToBuy;
        }

        public string CorrelationId { get; set; }

        public string BookId { get; set; }

        public int Quantity { get; set; }

        public bool WantToBuy { get; set; }
    }
}