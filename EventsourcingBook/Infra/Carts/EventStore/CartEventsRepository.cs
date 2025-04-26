global using CartEventStored = Deciders.DeciderEventInterpreter<
    EventsourcingBook.Domain.Carts.CartId,
    EventsourcingBook.Domain.Carts.CartCommand,
    EventsourcingBook.Domain.Carts.CartEvent,
    EventsourcingBook.Domain.Carts.CartState,
    EventsourcingBook.Domain.Carts.CartError>;

using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Infra.Common;
using EventStore.Client;

namespace EventsourcingBook.Infra.Carts.EventStore;

public class CartEventsRepository(EventStoreClient client)
    : EventsRepository<CartId, CartEvent>(streamPrefix: "Cart", client);
