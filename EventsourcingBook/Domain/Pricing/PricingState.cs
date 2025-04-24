namespace EventsourcingBook.Domain.Inventories;

public record PricingState(decimal Price)
{
    public static PricingState Evolve(PricingState state, PricingEvent @event)
    {
        switch (@event)
        {
            case PricingEvent.ProductPriceChangedEvent ev:
                return new PricingState(ev.NewPrice);

            default:
                return state;
        }
    }
}
