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

    public sealed record ItemRemovedEvent(
        CartItemId ItemId)
        : CartEvent;

    public sealed record CartCleared()
        : CartEvent;

    public sealed record ItemArchivedEvent(
        CartItemId ItemId)
        : CartEvent;
}
