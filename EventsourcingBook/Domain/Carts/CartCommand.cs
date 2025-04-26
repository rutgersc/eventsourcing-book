using EventsourcingBook.Domain.Products;

namespace EventsourcingBook.Domain.Carts;

public abstract record CartCommand
{
    private CartCommand() { }

    public sealed record AddItemCommand(
        string Description,
        string Image,
        decimal Price,
        Guid ItemId,
        Guid ProductId,
        DeviceFingerPrint FingerPrint)
        : CartCommand;

    public sealed record RemoveItemCommand(
        CartItemId ItemId)
        : CartCommand;

    public sealed record ClearCartCommand()
        : CartCommand;

    public sealed record ArchiveItemCommand(
        ProductId ProductId)
        : CartCommand;

    public sealed record SubmitCartCommand
        : CartCommand;

    public sealed record PublishCartCommand(
        List<CartEvent.OrderedProduct> OrderedProducts,
        decimal TotalPrice)
        : CartCommand;

    // NOTE: not in the original
    public sealed record PublishCartFailedCommand()
        : CartCommand;
}
