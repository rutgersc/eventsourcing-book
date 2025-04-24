using System.Collections.Immutable;
using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Domain.Products;

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
                .Select(productByCartItem =>
                {
                    var cartItem = CartItems.FirstOrDefault(
                        existinItemId => existinItemId.ItemId == productByCartItem.Value.Value,
                        new CartItem());

                    cartItem.ItemId = productByCartItem.Key.Value;
                    cartItem.ProductId = productByCartItem.Value.Value;
                    return cartItem;
                })
                .ToList();
        }
    }

    public CartState.Cart ToDomain()
    {
        return new CartState.Cart(
            Items: CartItems
                .ToImmutableDictionary(
                    item => new CartItemId(item.ItemId),
                    item => new ProductId(item.ProductId)));
    }
}

public class CartItem
{
    public Guid CartItemId { get; set; }

    public Guid ItemId { get; set; }

    public Guid CartId { get; set; }

    public Guid ProductId { get; set; }
}
