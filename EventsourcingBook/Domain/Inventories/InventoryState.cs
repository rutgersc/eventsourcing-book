namespace EventsourcingBook.Domain.Inventories;

public record InventoryState(int Inventory)
{
    public static InventoryState InitialState => new(0);

    public static InventoryState Evolve(InventoryState state, InventoryEvent @event)
    {
        switch (@event)
        {
            case InventoryEvent.InventoryChangedEvent ev:
                return state with { Inventory = ev.Inventory };

            default:
                return state;
        }
    }
}
