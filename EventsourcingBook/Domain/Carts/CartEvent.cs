using EventsourcingBook.Domain.Products;

namespace EventsourcingBook.Domain.Carts;

public abstract record CartEvent
{
    private CartEvent() { }

    public sealed record CartCreatedEvent()
        : CartEvent;

    public sealed record ItemAddedEvent(
        string Description,
        string Image,
        decimal Price,
        CartItemId ItemId,
        ProductId ProductId)
        : CartEvent;
}
