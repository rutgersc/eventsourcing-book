namespace EventsourcingBook.Infra.Inventories;

public class CartsWithProductsReadModelEntity
{
    public Guid CartId { get; set; } // aggregateId in the original

    public Guid ProductId { get; set; }
}
