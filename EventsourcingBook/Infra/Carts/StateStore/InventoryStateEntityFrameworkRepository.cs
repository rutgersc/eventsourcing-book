using Deciders;
using EventsourcingBook.Domain.Inventories;
using EventsourcingBook.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace EventsourcingBook.Infra.Carts;

public class InventoryStateEntityFrameworkRepository(AppDbContext dbContext)
    : IDeciderStatePersistence<ProductId, InventoryState>
{
    public async Task<InventoryState> LoadState(ProductId id, InventoryState initialState)
    {
        var inventory = await dbContext.Inventories.FirstOrDefaultAsync(cart => cart.ProductId == id.Value);
        return inventory?.ToDomain() ?? initialState;
    }

    public async Task<ProductId> SaveState(ProductId id, InventoryState state)
    {
        var inventory = await dbContext.Inventories.FirstOrDefaultAsync(cart => cart.ProductId == id.Value);

        if (inventory == null)
        {
            inventory = new InventoryEntity { ProductId = id.Value };
            dbContext.Inventories.Add(inventory);
        }

        inventory.ApplyState(state);

        await dbContext.SaveChangesAsync();

        return new ProductId(inventory.ProductId);
    }
}
