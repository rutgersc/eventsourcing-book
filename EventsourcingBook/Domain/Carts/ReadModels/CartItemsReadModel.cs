using System.Collections.Immutable;
using Deciders;
using EventsourcingBook.Domain.Carts.Upcaster;
using static EventsourcingBook.Domain.Carts.CartEvent;

namespace EventsourcingBook.Domain.Carts.ReadModels;

public record CartItemsReadModel(
    ImmutableList<CartItem> CartItems,
    decimal TotalPrice)
{
    public static StateView<CartEvent, CartItemsReadModel> StateView = new(
        Evolve: Evolve,
        InitialState: new CartItemsReadModel([], 0));

    public static CartItemsReadModel Evolve(CartItemsReadModel state, CartEvent @event)
    {
        var upcastedEvent = CartEvent.Upcast(@event);

        switch (upcastedEvent)
        {
            case CartCreatedEvent ev:
                return new CartItemsReadModel(
                    CartItems: [],
                    TotalPrice: 0);

            case ItemAddedEventV1:
                throw new NotImplementedException("forgot to upcast?");

            case ItemAddedEvent ev:
                return state with
                {
                    CartItems = state.CartItems.Add(CartItem.FromEvent(ev)),
                    TotalPrice = state.TotalPrice + ev.Price
                };

            case ItemRemovedEvent ev:
                var itemToRemove = state.CartItems.First(item => item.ItemId == ev.ItemId.Value);
                return state with
                {
                    CartItems = state.CartItems.Remove(itemToRemove),
                    TotalPrice = state.TotalPrice - itemToRemove.Price
                };

            case CartCleared ev:
                return state with
                {
                    CartItems = state.CartItems.Clear(),
                    TotalPrice = 0
                };

            default:
                return state;
        }
    }
}


public record CartItem(
    Guid ItemId,
    // Guid CartId,
    string Description,
    string Image,
    decimal Price,
    Guid ProductId,
    DeviceFingerPrint FingerPrint)
{
    public static CartItem FromEvent(ItemAddedEvent ev)
    {
        return new CartItem(
            Description: ev.Description,
            Image: ev.Image,
            Price: ev.Price,
            ItemId: ev.ItemId.Value,
            ProductId: ev.ProductId.Value,
            FingerPrint: ev.FingerPrint);
    }
};
