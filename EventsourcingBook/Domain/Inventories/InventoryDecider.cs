using Deciders;
using EventsourcingBook.Domain.Products;

namespace EventsourcingBook.Domain.Inventories;

public static class InventoryDecider
{
    public static Decider<ProductId, InventoryCommand, InventoryEvent, InventoryState, InventoryError> Decider = new(
        Decide: Decide,
        Evolve: InventoryState.Evolve,
        InitialState: InventoryState.InitialState);

    public static Result<IReadOnlyCollection<InventoryEvent>, InventoryError> Decide(InventoryCommand command, InventoryState state)
    {
        switch (command, state)
        {
            case (InventoryCommand.ChangeInventoryCommand cmd, _):
                return new InventoryEvent[] { new InventoryEvent.InventoryChangedEvent(cmd.Inventory) };

            default:
                return new InventoryEvent[] { };
        }
    }
}
