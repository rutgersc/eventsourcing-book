using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Domain.Inventories;
using EventsourcingBook.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace EventsourcingBook.Infra;

public class ArchiveItemAutomationProcessor
{
    public static async Task React(
        AppDbContext dbContext,
        CartEventStored cartEventStored,
        ProductId productId,
        PricingEvent @event)
    {
        if (@event is PricingEvent.ProductPriceChangedEvent ev)
        {
            var carts = await dbContext.CartsWithProducts
                .Where(cart => cart.ProductId == productId.Value)
                .ToListAsync();

            foreach (var cart in carts)
            {
                await cartEventStored.ExecuteCommand(new CartId(cart.CartId), new CartCommand.ArchiveItemCommand(productId));
            }
        }
    }
}
