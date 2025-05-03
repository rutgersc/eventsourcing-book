using System.Collections.Immutable;
using EventsourcingBook.Domain.Carts;

namespace EventsourcingBook.Infra.Carts;

public class Cart
{
    public Guid CartId { get; set; }

    public List<CartItem> CartItems { get; set; } = new();

    public void ApplyState(CartState cartState)
    {
        if (cartState is CartState.Cart cart)
        {
            this.CartItems = cart.Items
                .Select(itemId =>
                    CartItems.FirstOrDefault(
                        existinItemId => existinItemId.CartItemId == itemId.Value,
                        new CartItem() { CartItemId = itemId.Value }))
                .ToList();
        }
    }

    public CartState.Cart ToDomain()
    {
        return new CartState.Cart(
            Items: CartItems
                .Select(item => new CartItemId(item.CartItemId))
                .ToImmutableHashSet());
    }
}

public class CartItem
{
    public Guid CartItemId { get; set; }

    public Guid CartId { get; set; }
}
