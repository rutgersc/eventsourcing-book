using System.Collections.Immutable;
using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Domain.Products;

namespace EventsourcingBook.Infra.Carts;

public class Cart
{
    public Guid CartId { get; set; }

    public List<CartItem> CartItems { get; set; } = new();

    public SubmittedCart? SubmittedCart { get; set; }

    public PublishedCart? PublishedCart { get; set; }

    public void ApplyState(CartState.Cart cart)
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

    public void ApplyState(CartState.SubmittedCart submittedState)
    {
        this.SubmittedCart ??= new SubmittedCart();
        this.SubmittedCart.TotalPrice = submittedState.TotalPrice;
        this.SubmittedCart.OrderedProducts = submittedState.OrderedProducts
            .Select(v =>
            {
                var orderedProduct = this.SubmittedCart.OrderedProducts
                    .FirstOrDefault(
                        t => t.ProductId == v.ProductId.Value,
                        new OrderedProducts());

                orderedProduct.ProductId = v.ProductId.Value;
                orderedProduct.Price = v.Price;
                return orderedProduct;
            })
            .ToList();
        this.SubmittedCart.PublicationFailed = submittedState.PublicationFailed;
    }

    public void ApplyState(CartState.PublishedCart publishedCart)
    {
        this.PublishedCart ??= new PublishedCart();
    }

    public void ApplyState(CartState cartState)
    {
        switch (cartState)
        {
            case CartState.CartInitialState:
                break;

            case CartState.Cart cart:
                this.ApplyState(cart);
                break;

            case CartState.SubmittedCart submittedState:
                this.ApplyState(submittedState.UnsubmittedCart);
                this.ApplyState(submittedState);
                break;

            case CartState.PublishedCart publishedCart:
                this.ApplyState(publishedCart);
                break;
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

        var submittedState = new CartState.SubmittedCart(
            cart,
            OrderedProducts: this.SubmittedCart.OrderedProducts
                .Select(op => new CartEvent.OrderedProduct(new ProductId(op.ProductId), op.Price))
                .ToList(),
            TotalPrice: this.SubmittedCart.TotalPrice,
            PublicationFailed: this.SubmittedCart.PublicationFailed);

        return this.PublishedCart == null
            ? submittedState
            : new CartState.PublishedCart(submittedState);
    }
}

public class CartItem
{
    public Guid CartItemId { get; set; }

    public Guid ItemId { get; set; }

    public Guid ProductId { get; set; }

    public Guid CartId { get; set; }

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

public class PublishedCart
{
    public Guid CartId { get; set; }
}
