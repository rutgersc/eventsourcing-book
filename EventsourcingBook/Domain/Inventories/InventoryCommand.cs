namespace EventsourcingBook.Domain.Inventories;

public abstract record InventoryCommand
{
    private InventoryCommand() { }

    public sealed record ChangeInventoryCommand(
        int Inventory)
        : InventoryCommand;
}
