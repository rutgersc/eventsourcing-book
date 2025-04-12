using Deciders;
using EventsourcingBook.Domain.Carts;

namespace EventsourcingBook.Infra.Carts;

public class CartStateEntityFrameworkRepository(AppStateDbContext dbContext)
    : IDeciderStatePersistence<CartId, CartState>
{
    public Task<CartState> LoadState(CartId id, CartState initialState)
    {
        throw new NotImplementedException();
    }

    public Task<CartId> SaveState(CartId id, CartState state)
    {
        throw new NotImplementedException();
    }
}
