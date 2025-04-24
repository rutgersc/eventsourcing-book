namespace EventsourcingBook.Domain.Inventories;

public abstract record PricingCommand
{
    private PricingCommand() { }

    public sealed record ChangePriceCommand(
        decimal OldPrice,
        decimal NewPrice)
        : PricingCommand;
}
