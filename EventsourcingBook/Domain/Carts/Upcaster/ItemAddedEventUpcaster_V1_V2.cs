namespace EventsourcingBook.Domain.Carts.Upcaster;

public class ItemAddedEventUpcaster_V1_V2
{
    public static CartEvent Upcast(CartEvent ev)
    {
        if (ev is CartEvent.ItemAddedEventV1 itemAdded)
        {
            return new CartEvent.ItemAddedEvent(
                itemAdded.Description,
                itemAdded.Image,
                itemAdded.Price,
                itemAdded.ItemId,
                itemAdded.ProductId,
                DeviceFingerPrint.Default);
        }

        return ev;
    }
}
