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
            default:
                return state;
        }
    }

}

