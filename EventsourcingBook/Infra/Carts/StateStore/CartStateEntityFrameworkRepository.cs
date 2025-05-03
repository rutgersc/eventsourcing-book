using Deciders;
using EventsourcingBook.Domain.Carts;
using Microsoft.EntityFrameworkCore;

namespace EventsourcingBook.Infra.Carts;

public class CartStateEntityFrameworkRepository(AppDbContext dbContext)
    : IDeciderStatePersistence<CartId, CartState>
{
    public async Task<CartState> LoadState(CartId id, CartState initialState)
    {
        var cart = await dbContext.Carts.FirstOrDefaultAsync(cart => cart.CartId == id.Value);
        return cart?.ToDomain() ?? initialState;
    }

    public async Task<CartId> SaveState(CartId id, CartState state)
    {
        var cart = await dbContext.Carts.FirstOrDefaultAsync(cart => cart.CartId == id.Value);

        if (cart == null)
        {
            cart = new Cart();
            dbContext.Carts.Add(cart);
        }

        cart.ApplyState(state);

        await dbContext.SaveChangesAsync();

        return new CartId(cart.CartId);
    }
}
