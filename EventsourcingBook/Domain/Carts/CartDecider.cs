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
                    ProductId: new ProductId(addItemCommand.ProductId),
                    FingerPrint: addItemCommand.FingerPrint);

                return state is CartInitialState
                    ? new CartEvent[] { new CartCreatedEvent(), itemAddedEvent }
                    : new CartEvent[] { itemAddedEvent };

            case (RemoveItemCommand removeItemCommand, Cart cart):
                if (!cart.Items.ContainsKey(removeItemCommand.ItemId))
                {
                    return new CartItemIsNotInCart();
                }

                return new CartEvent[]
                {
                    new ItemRemovedEvent(removeItemCommand.ItemId)
                };

            case (_, CartInitialState):
                return new CartNotFound();

            case (ClearCartCommand, Cart):
                return new CartEvent[] { new CartCleared() };

            case (ArchiveItemCommand cmd, Cart cart):
                foreach (var kvp in cart.Items.Where(kvp => kvp.Value == cmd.ProductId))
                {
                    return new CartEvent[] { new ItemRemovedEvent(kvp.Key) };
                }

                return new CartEvent[] { };


            case (SubmitCartCommand cmd, SubmittedCart):
                return new CannotSubmitEmptyCart();

            case (SubmitCartCommand cmd, Cart cart):
                if (cart.Items.IsEmpty)
                {
                    return new CannotSubmitEmptyCart();
                }

                var submittedEvent = new CartSubmittedEvent(
                    OrderedProducts: cart.Items.Values
                        .Select(productId => new OrderedProduct(productId, cart.ProductPrices[productId]))
                        .ToList(),
                    TotalPrice: cart.Items.Values.Sum(productId => cart.ProductPrices[productId]) );

                return new CartEvent[] { submittedEvent };


            case (PublishCartCommand, Cart):
                return new CannotPublishUnsubmittedCart();

            case (PublishCartCommand, PublishedCart):
                return new CannotPublishCartTwice();

            case (PublishCartCommand, SubmittedCart):
                return new CartEvent[] { new CartPublishedEvent() };

            case (PublishCartFailedCommand, SubmittedCart):
                return new CartEvent[] { new CartPublicationFailedEvent() };

            default:
                return new CartEvent[] { };
        }
    }
}
