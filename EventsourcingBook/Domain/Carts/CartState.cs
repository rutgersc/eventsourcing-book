using System.Collections.Immutable;
using static EventsourcingBook.Domain.Carts.CartEvent;

namespace EventsourcingBook.Domain.Carts;

public abstract record CartState
{
    private CartState() { }

    public sealed record CartInitialState
        : CartState;

    public sealed record Cart(ImmutableHashSet<CartItemId> Items)
        : CartState;

    public static CartState Evolve(CartState state, CartEvent @event)
    {
        switch (state, @event)
        {
            case (CartInitialState, CartCreatedEvent):
                return new Cart([]);

            case (Cart cart, ItemAddedEvent addedEvent):
                return cart with { Items = cart.Items.Add(addedEvent.ItemId) };

            case (Cart cart, ItemRemovedEvent removedEvent):
                return cart with { Items = cart.Items.Remove(removedEvent.ItemId) };

            case (Cart cart, CartCleared):
                return cart with { Items = cart.Items.Clear() };

            default:
                return state;
        }
    }

}

