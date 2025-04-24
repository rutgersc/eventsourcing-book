namespace EventsourcingBook.Domain.Inventories;

public abstract record PricingEvent
{
    private PricingEvent() { }

    public sealed record ProductPriceChangedEvent(decimal OldPrice, decimal NewPrice)
        : PricingEvent;
}
