namespace EventsourcingBook.Domain.Carts;

public abstract record CartError
{
    private CartError() { }

    public sealed record CartNotFound() : CartError;

    public sealed record CartItemSizeExceeded() : CartError;
}
