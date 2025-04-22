namespace EventsourcingBook.Infra.Inventories;

public class InventoriesReadModelEntity
{
    public Guid ProductId { get; set; }

    public int Inventory { get; set; }
}
