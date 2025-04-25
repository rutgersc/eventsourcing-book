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
        Guid ProductId)
        : CartCommand;

    public sealed record RemoveItemCommand(
        CartItemId ItemId)
        : CartCommand;

    public sealed record ClearCartCommand()
        : CartCommand;

    public sealed record ArchiveItemCommand(
        ProductId ProductId)
        : CartCommand;
}
