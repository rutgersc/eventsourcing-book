using EventsourcingBook.Domain.Inventories;

namespace EventsourcingBook.Infra.Carts;

public class PricingEntity
{
    public Guid ProductId { get; set; }

    public decimal Price { get; set; }

    public PricingState ToDomain()
    {
        return new PricingState(this.Price);
    }

    public void ApplyState(PricingState state)
    {
        this.Price = state.Price;
    }
}

