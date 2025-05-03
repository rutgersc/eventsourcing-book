namespace EventsourcingBook.Domain.Carts;

public record CartId(Guid Value)
{
    public static bool TryParse(string? value, IFormatProvider? format, out CartId cartId)
    {
        if (Guid.TryParse(value, out var id))
        {
            cartId = new CartId(id);
            return true;
        }

        cartId = null!;
        return false;
    }

    public override string ToString() => Value.ToString();
}
