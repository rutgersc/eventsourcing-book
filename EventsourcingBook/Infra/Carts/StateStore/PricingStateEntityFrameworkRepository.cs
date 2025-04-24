using Deciders;
using EventsourcingBook.Domain.Inventories;
using EventsourcingBook.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace EventsourcingBook.Infra.Carts;

public class PricingStateEntityFrameworkRepository(AppDbContext dbContext)
    : IDeciderStatePersistence<ProductId, PricingState>
{
    public async Task<PricingState> LoadState(ProductId id, PricingState initialState)
    {
        var pricing = await dbContext.Pricing.FirstOrDefaultAsync(pricing => pricing.ProductId == id.Value);
        return pricing?.ToDomain() ?? initialState;
    }

    public async Task<ProductId> SaveState(ProductId id, PricingState state)
    {
        var pricing = await dbContext.Pricing.FirstOrDefaultAsync(pricing => pricing.ProductId == id.Value);

        if (pricing == null)
        {
            pricing = new PricingEntity { ProductId = id.Value };
            dbContext.Pricing.Add(pricing);
        }

        pricing.ApplyState(state);

        await dbContext.SaveChangesAsync();

        return new ProductId(pricing.ProductId);
    }
}
