using System.Collections.Immutable;
using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Domain.Products;

namespace EventsourcingBook.Infra.Carts;

public class Cart
{
    public Guid CartId { get; set; }

    public List<CartItem> CartItems { get; set; } = new();

    public SubmittedCart? SubmittedCart { get; set; }

    public void ApplyState(CartState cartState)
    {
        switch (cartState)
        {
            case CartState.CartInitialState:
                break;

            case CartState.Cart cart:
                ApplyCartState(cart);
                break;

            case CartState.SubmittedCart submittedState:
                ApplySubmittedState(submittedState);
                break;
        }

        return;

        void ApplyCartState(CartState.Cart cart)
        {
            this.CartItems = cart.Items
                .Select(productByCartItem =>
                {
                    var cartItem = CartItems.FirstOrDefault(
                        existinItemId => existinItemId.ItemId == productByCartItem.Value.Value,
                        new CartItem());

                    cartItem.ItemId = productByCartItem.Key.Value;
                    cartItem.ProductId = productByCartItem.Value.Value;
                    cartItem.Price = cart.ProductPrices[productByCartItem.Value];
                    return cartItem;
                })
                .ToList();
        }

        void ApplySubmittedState(CartState.SubmittedCart submittedState)
        {
            this.SubmittedCart ??= new SubmittedCart();
        }
    }

    public CartState ToDomain()
    {
        var cart = new CartState.Cart(
            Items: CartItems
                .ToImmutableDictionary(
                    item => new CartItemId(item.ItemId),
                    item => new ProductId(item.ProductId)),
            ProductPrices: CartItems
                .ToImmutableDictionary(
                    item => new ProductId(item.ProductId),
                    item => item.Price));

        if (this.SubmittedCart == null)
        {
            return cart;
        }

        var submittedState = new CartState.SubmittedCart();

        return submittedState;
    }
}

public class CartItem
{
    public Guid CartItemId { get; set; }

    public Guid ItemId { get; set; }

    public Guid CartId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Price { get; set; }
}


public class SubmittedCart
{
    public Guid CartId { get; set; }

    public decimal TotalPrice { get; set; }

    public List<OrderedProducts> OrderedProducts { get; set; } = new();

    public bool PublicationFailed { get; set; }

    public bool Published { get; set; }
}

public class OrderedProducts
{
    public Guid CartId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Price { get; set; }
}
