using EventsourcingBook.Domain.Inventories;
using EventsourcingBook.Domain.Products;
using EventsourcingBook.Infra.Common;
using EventStore.Client;

namespace EventsourcingBook.Infra.Inventories.EventStore;

public class PricingEventsRepository(EventStoreClient client)
    : EventsRepository<ProductId, PricingEvent>(streamPrefix: "Pricing", client);
