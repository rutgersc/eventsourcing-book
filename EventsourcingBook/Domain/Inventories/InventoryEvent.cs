namespace EventsourcingBook.Domain.Inventories;

public abstract record InventoryEvent
{
    private InventoryEvent() { }

    public sealed record InventoryChangedEvent(int Inventory)
        : InventoryEvent;
}
