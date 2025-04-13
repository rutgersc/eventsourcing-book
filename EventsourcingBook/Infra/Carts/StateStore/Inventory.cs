using EventsourcingBook.Domain.Inventories;

namespace EventsourcingBook.Infra.Carts;

public class InventoryEntity
{
    public Guid ProductId { get; set; }

    public int Inventory { get; set; }

    public InventoryState ToDomain()
    {
        return new InventoryState(this.Inventory);
    }

    public void ApplyState(InventoryState state)
    {
        this.Inventory = state.Inventory;
    }
}

