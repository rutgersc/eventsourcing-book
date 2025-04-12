using System.Collections.Immutable;
using Deciders;
using EventsourcingBook.Domain.Carts;

namespace EventsourcingBook.Infra.Carts;

public class CartStateInMemoryRepository
    : IDeciderStatePersistence<CartId, CartState>
{
    private ImmutableDictionary<CartId, CartState> carts = ImmutableDictionary<CartId, CartState>.Empty;

    public async Task<CartState> LoadState(CartId id, CartState initialState)
    {
        return carts.GetValueOrDefault(id, initialState);
    }

    public async Task<CartId> SaveState(CartId id, CartState state)
    {
        carts = carts.Add(id, state);
        return id;
    }
}
