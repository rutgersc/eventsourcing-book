namespace EventsourcingBook.Domain.Carts;

public abstract record CartError
{
    private CartError() { }

    public sealed record CartNotFound() : CartError;

    public sealed record CartItemSizeExceeded() : CartError;

    public sealed record CartItemIsNotInCart() : CartError;

    public sealed record CannotSubmitEmptyCart : CartError;

    public sealed record CannotPublishUnsubmittedCart : CartError;

    public sealed record CannotPublishCartTwice : CartError;
}
