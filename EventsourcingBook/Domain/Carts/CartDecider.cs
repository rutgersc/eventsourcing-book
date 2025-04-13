using Deciders;
using EventsourcingBook.Domain.Products;
using static EventsourcingBook.Domain.Carts.CartCommand;
using static EventsourcingBook.Domain.Carts.CartError;
using static EventsourcingBook.Domain.Carts.CartEvent;
using static EventsourcingBook.Domain.Carts.CartState;

namespace EventsourcingBook.Domain.Carts;

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
            case (AddItemCommand addItemCommand, _):
                if (state is Cart { Items.Count: >= 3 })
                {
                    return new CartItemSizeExceeded();
                }

                var itemAddedEvent = new ItemAddedEvent(
                    Description: addItemCommand.Description,
                    Image: addItemCommand.Image,
                    Price: addItemCommand.Price,
                    ItemId: new CartItemId(addItemCommand.ItemId),
                    ProductId: new ProductId(addItemCommand.ProductId));

                return state is CartInitialState
                    ? new CartEvent[] { new CartCreatedEvent(), itemAddedEvent }
                    : new CartEvent[] { itemAddedEvent };

            case (RemoveItemCommand removeItemCommand, Cart cart):
                return new CartEvent[]
                {
                };

            case (_, CartInitialState):
                return new CartNotFound();

            default:
                return new CartEvent[] { };
        }
    }
}
