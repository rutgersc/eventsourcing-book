using EventsourcingBook.Domain.Inventories;
using EventsourcingBook.Domain.Products;
using EventsourcingBook.Infra.Common;
using EventStore.Client;

namespace EventsourcingBook.Infra.Inventories.EventStore;

public class InventoryEventsRepository(EventStoreClient client)
    : EventsRepository<ProductId, InventoryEvent>(streamPrefix: "Inventory", client);
