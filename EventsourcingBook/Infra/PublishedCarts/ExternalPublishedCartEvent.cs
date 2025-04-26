
using EventsourcingBook.Domain.Carts;

namespace EventsourcingBook.Infra.PublishedCarts;

public sealed record OrderedProduct(
    Guid ProductId,
    decimal Price);

public class ExternalPublishedCartEvent(
    Guid CartId,
    decimal TotalPrice,
    List<OrderedProduct> OrderedProducts)
{
    public static ExternalPublishedCartEvent FromCartCommand(CartId id, CartCommand.PublishCartCommand cmd)
    {
        return new(
            CartId: id.Value,
            TotalPrice: cmd.TotalPrice,
            OrderedProducts: cmd.OrderedProducts.Select(p => new OrderedProduct(p.ProductId.Value, p.Price)).ToList());
    }
}
