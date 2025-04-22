using EventsourcingBook.Domain.Inventories;
using EventsourcingBook.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace EventsourcingBook.Infra.Inventories;

public static class InventoriesReadModelProjector
{
    public static async Task Project(AppDbContext dbContext, ProductId id, InventoryEvent @event)
    {
        var entity = await dbContext.InventoriesReadModel.FirstOrDefaultAsync(i => i.ProductId == id.Value);

        if (entity == null)
        {
            entity = new InventoriesReadModelEntity { ProductId = id.Value };
        }

        entity = new [] { @event }.Aggregate(entity, Evolve);

        await dbContext.SaveChangesAsync();
    }

    public static InventoriesReadModelEntity Evolve(InventoriesReadModelEntity state, InventoryEvent @event)
    {
        switch (@event)
        {
            case InventoryEvent.InventoryChangedEvent ev:
                state.Inventory = ev.Inventory;
                break;
        }

        return state;
    }
}
