using EventsourcingBook.Domain.Carts;
using Microsoft.EntityFrameworkCore;
using static EventsourcingBook.Domain.Carts.CartEvent;

namespace EventsourcingBook.Infra.Inventories;

public static class CartsWithProductsReadModelProjector
{
    public static async Task Project(AppDbContext dbContext, CartId id, CartEvent @event)
    {
        switch (@event)
        {
            case CartCreatedEvent:
                break;

            case CartCleared:
                var entitiesToRemove = await dbContext.CartsWithProducts
                    .Where(entity => entity.CartId == id.Value)
                    .ToListAsync();

                dbContext.CartsWithProducts.RemoveRange(entitiesToRemove);
                break;

            case ItemAddedEvent ev:
                dbContext.CartsWithProducts.Add(new CartsWithProductsReadModelEntity
                {
                    CartId = id.Value,
                    ProductId = ev.ProductId.Value
                });
                break;

            case ItemRemovedEvent ev:
                var entityToRemove = await dbContext.CartsWithProducts
                    .FirstOrDefaultAsync(entity => entity.CartId == id.Value && entity.ProductId == ev.ItemId.Value);

                if (entityToRemove != null)
                {
                    dbContext.CartsWithProducts.Remove(entityToRemove);
                }

                break;

            case ItemArchivedEvent ev:
                var entity = await dbContext.CartsWithProducts
                    .FirstOrDefaultAsync(entity => entity.CartId == id.Value && entity.ProductId == ev.ItemId.Value);

                if (entity != null)
                {
                    dbContext.CartsWithProducts.Remove(entity);
                }

                break;
        }

        await dbContext.SaveChangesAsync();
    }
}
