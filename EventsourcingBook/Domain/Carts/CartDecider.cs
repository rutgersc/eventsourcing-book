using Deciders;
using static EventsourcingBook.Domain.Carts.CartCommand;
using static EventsourcingBook.Domain.Carts.CartEvent;
using static EventsourcingBook.Domain.Carts.CartState;

namespace EventsourcingBook.Domain.Carts;

public record CartId(Guid Value);

public sealed record CartItemId(Guid Value);

public static class CartDecider
{
    public static Decider<CartId, CartCommand, CartEvent, CartState, CartError> Decider = new(
        Decide: Decide,
        Evolve: Evolve,
        InitialState: new CartInitialState());

    public static Result<IReadOnlyCollection<CartEvent>, CartError> Decide(CartCommand command, CartState state)
    {
        switch (command, state)
        {

            default:
                return new CartEvent[] { };
        }
    }
}
