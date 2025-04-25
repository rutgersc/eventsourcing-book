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
        ImmutableDictionary<CartItemId, ProductId> Items)
        : CartState;

    public static CartState Evolve(CartState state, CartEvent @event)
    {
        switch (state, @event)
        {
            case (CartInitialState, CartCreatedEvent):
                return new Cart(
                    Items: ImmutableDictionary<CartItemId, ProductId>.Empty);

            case (Cart cart, ItemAddedEvent addedEvent):
                return cart with
                {
                    Items = cart.Items.Add(addedEvent.ItemId, addedEvent.ProductId)
                };

            case (Cart cart, ItemRemovedEvent removedEvent):
                return cart with
                {
                    Items = cart.Items.Remove(removedEvent.ItemId)
                };

            case (Cart cart, CartCleared):
                return cart with
                {
                    Items = cart.Items.Clear()
                };

            case (Cart cart, ItemArchivedEvent):
                return cart with { Items = cart.Items.Clear() };

            default:
                return state;
        }
    }

}

