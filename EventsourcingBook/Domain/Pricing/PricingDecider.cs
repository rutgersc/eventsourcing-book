using Deciders;
using EventsourcingBook.Domain.Products;

namespace EventsourcingBook.Domain.Inventories;

public static class PricingDecider
{
    public static Decider<ProductId, PricingCommand, PricingEvent, PricingState, PricingError> Decider = new(
        Decide: Decide,
        Evolve: PricingState.Evolve,
        InitialState: new PricingState(-1));

    public static Result<IReadOnlyCollection<PricingEvent>, PricingError> Decide(PricingCommand command, PricingState state)
    {
        switch (command, state)
        {
            case (PricingCommand.ChangePriceCommand cmd, _):
                return new PricingEvent[] { new PricingEvent.ProductPriceChangedEvent(cmd.OldPrice, cmd.NewPrice) };

            default:
                return new PricingEvent[] { };
        }
    }
}
