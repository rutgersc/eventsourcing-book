using System.Collections.Immutable;
using EventsourcingBook.Domain.Products;
using static EventsourcingBook.Domain.Carts.CartEvent;

namespace EventsourcingBook.Domain.Carts;

public abstract record CartState
{
    private CartState() { }

    public sealed record CartInitialState
        : CartState;

    public sealed record Cart(
        ImmutableDictionary<CartItemId, ProductId> Items,
        ImmutableDictionary<ProductId, decimal> ProductPrices)
        : CartState;

    public sealed record SubmittedCart(
        Cart UnsubmittedCart,
        List<OrderedProduct> OrderedProducts,
        decimal TotalPrice,
        bool PublicationFailed)
        : CartState;

    public sealed record PublishedCart(
        SubmittedCart UnpublishedCart)
        : CartState;

    public static CartState Evolve(CartState state, CartEvent @event)
    {
        switch (state, @event)
        {
            case (CartInitialState, CartCreatedEvent):
                return new Cart(
                    Items: ImmutableDictionary<CartItemId, ProductId>.Empty,
                    ProductPrices: ImmutableDictionary<ProductId, decimal>.Empty);

            case (Cart cart, ItemAddedEvent addedEvent):
                return cart with
                {
                    Items = cart.Items.Add(addedEvent.ItemId, addedEvent.ProductId) ,
                    ProductPrices = cart.ProductPrices.Add(addedEvent.ProductId, addedEvent.Price)
                };

            case (Cart cart, ItemRemovedEvent ev):
                return cart with
                {
                    Items = cart.Items.Remove(ev.ItemId),
                    ProductPrices = cart.ProductPrices.Remove(cart.Items[ev.ItemId])
                };

            case (Cart cart, CartCleared):
                return cart with
                {
                    Items = cart.Items.Clear(),
                    ProductPrices = cart.ProductPrices.Clear()
                };

            case (Cart cart, ItemArchivedEvent ev):
                return cart with
                {
                    Items = cart.Items.Remove(ev.ItemId),
                    ProductPrices = cart.ProductPrices.Remove(cart.Items[ev.ItemId])
                };

            case (Cart cart, CartSubmittedEvent ev):
                return new SubmittedCart(
                    cart,
                    ev.OrderedProducts,
                    ev.TotalPrice,
                    PublicationFailed: false);

            case (SubmittedCart submittedCart, CartPublicationFailedEvent):
                return submittedCart with { PublicationFailed = true };

            case (SubmittedCart submittedCart, CartPublishedEvent):
                return new PublishedCart(submittedCart);

            default:
                return state;
        }
    }

}

