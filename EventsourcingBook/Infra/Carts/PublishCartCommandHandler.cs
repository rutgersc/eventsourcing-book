using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Infra.PublishedCarts;

namespace EventsourcingBook.Infra.Inventories;

public class PublishCartCommandHandler
{
    public static async Task<Result<IReadOnlyCollection<CartEvent>, CartError>> Handle(
        CartEventStored cartEventStored,
        MockEventPublisher eventPublisher,
        CartId cartId,
        CartCommand.PublishCartCommand publishCartCommand)
    {
        // NOTE: dual write problem. Transactional outbox would be more reliable
        var kafkaTransaction = MockTransaction.Create();
        var eventStoreTransaction =  MockTransaction.Create();

        try
        {
            // external system 1
            var outgoingExternalEvent = ExternalPublishedCartEvent.FromCartCommand(cartId, publishCartCommand);
            await eventPublisher.Publish("published_carts", outgoingExternalEvent);

            // external system 2
            var result = await cartEventStored.ExecuteCommand(cartId, publishCartCommand);
            if (result.TryPickError(out var cartError, out _))
            {
                throw new Exception($"business logic prevents publishing of cart: {cartError}");
            }

            // What if one of these 2 is unavailable after the first tx commits?
            eventStoreTransaction.Commit();
            kafkaTransaction.Commit();
            return result;
        }
        catch
        {
            kafkaTransaction.Rollback();
            eventStoreTransaction.Rollback();

            // Note that this may fail too
            return await cartEventStored.ExecuteCommand(cartId, new CartCommand.PublishCartFailedCommand());
        }
    }
}
