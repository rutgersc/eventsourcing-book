using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Infra.PublishedCarts;

using CartDispatch = System.Func<EventsourcingBook.Domain.Carts.CartId, EventsourcingBook.Domain.Carts.CartCommand, System.Threading.Tasks.Task<System.Result<System.Collections.Generic.IReadOnlyCollection<EventsourcingBook.Domain.Carts.CartEvent>, EventsourcingBook.Domain.Carts.CartError>>>;

namespace EventsourcingBook.Infra.Inventories;

public class PublishCartCommandHandler
{
    public static async Task<Result<IReadOnlyCollection<CartEvent>, CartError>> Handle(
        CartDispatch cartDispatch,
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
            var result = await cartDispatch(cartId, publishCartCommand);
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
            return await cartDispatch(cartId, new CartCommand.PublishCartFailedCommand());
        }
    }
}
