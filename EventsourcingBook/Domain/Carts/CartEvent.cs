 using EventsourcingBook.Domain.Products;
 using EventsourcingBook.Domain.Carts.Upcaster;

namespace EventsourcingBook.Domain.Carts;

public abstract record CartEvent
{
    private CartEvent() { }

    public static CartEvent Upcast(CartEvent ev)
    {
        // list all upcasters
        return ItemAddedEventUpcaster_V1_V2.Upcast(ev);
    }

    public sealed record CartCreatedEvent()
        : CartEvent;

    public sealed record ItemAddedEventV1(
        string Description,
        string Image,
        decimal Price,
        CartItemId ItemId,
        ProductId ProductId)
        : CartEvent;

    public sealed record ItemAddedEvent(
        string Description,
        string Image,
        decimal Price,
        CartItemId ItemId,
        ProductId ProductId,
        DeviceFingerPrint FingerPrint)
        : CartEvent;

    public sealed record ItemRemovedEvent(
        CartItemId ItemId)
        : CartEvent;

    public sealed record CartCleared()
        : CartEvent;

    public sealed record ItemArchivedEvent(
        CartItemId ItemId)
        : CartEvent;

    public sealed record CartSubmittedEvent(
        List<OrderedProduct> OrderedProducts,
        decimal TotalPrice)
        : CartEvent;

    public sealed record CartPublishedEvent()
        : CartEvent;

    public sealed record CartPublicationFailedEvent()
        : CartEvent;

    public sealed record OrderedProduct(ProductId ProductId, decimal Price);
}
