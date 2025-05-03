namespace EventsourcingBook.Domain.Products;

public record ProductId(Guid Value)
{
    public static bool TryParse(string value, IFormatProvider format, out ProductId productId)
    {
        if (Guid.TryParse(value, out var id))
        {
            productId = new ProductId(id);
            return true;
        }

        productId = null!;
        return false;
    }

    public override string ToString() => Value.ToString();
}
