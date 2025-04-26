namespace EventsourcingBook.Domain;

public sealed record DeviceFingerPrint(string Value)
{
    public static DeviceFingerPrint Calculate()
        => new(Guid.NewGuid().ToString());

    public static DeviceFingerPrint Default =>
        new("default-fingerprint");
}
