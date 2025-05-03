using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Infra.Common;
using EventStore.Client;

namespace EventsourcingBook.Infra.Carts.EventStore;

public class CartEventsRepository(EventStoreClient client)
    : EventsRepository<CartId, CartEvent>(streamPrefix: "Cart", client);
