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
}
