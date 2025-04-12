using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

using Deciders;
using EventsourcingBook.Domain.Carts;
using EventStore.Client;

namespace EventsourcingBook.Infra.EventStore.Carts;

public class CartEventsRepository(string? connectionString)
    : IDeciderEventPersistence<CartId, CartEvent>
{
    private readonly EventStoreClientSettings settings = EventStoreClientSettings.Create(connectionString ?? "esdb://localhost:2113?tls=false");
    private EventStoreClient client => new(settings);
    private readonly EventTypeMapping eventMapping = new();

    public async Task<IReadOnlyCollection<CartEvent>?> LoadEvents(CartId id)
    {
        var streamName = $"Cart-{id}";
        var result = client.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start);

        try
        {
            var events = await result.ToListAsync();
            return events.Select(Deserialize<CartEvent>).ToList();
        }
        catch (StreamNotFoundException)
        {
            return null;
        }
    }

    public async Task SaveEvents(CartId id, IReadOnlyCollection<CartEvent> events)
    {
        var eventData = events.Select(Serialize).ToList();

        await client.AppendToStreamAsync(
            streamName: $"Cart-{id}",
            StreamState.Any,
            eventData: eventData);
    }

    EventData Serialize(CartEvent @event)
    {
        return new EventData(
            eventId: Uuid.NewUuid(),
            type: eventMapping.ToName(@event.GetType()),
            data: JsonSerializer.SerializeToUtf8Bytes(@event,  @event.GetType()));
    }

    T Deserialize<T>(ResolvedEvent resolvedEvent)
    {
        var dataType = eventMapping.ToType<T>(resolvedEvent.Event.EventType)!;
        var jsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
        var data = JsonSerializer.Deserialize(jsonData, dataType);
        return (T)data!;
    }
}


public class EventTypeMapping
{
    private readonly ConcurrentDictionary<string, Type?> typeMap = new();
    private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

    public string ToName(Type eventType) =>
        typeNameMap.GetOrAdd(eventType, _ =>
        {
            var eventTypeName = eventType.Name!;

            typeMap.TryAdd(eventTypeName, eventType);

            return eventTypeName;
        });

    public Type? ToType<T>(string eventTypeName) =>
        typeMap.GetOrAdd(eventTypeName, _ =>
        {
            var type = GetFirstMatchingTypeFromCurrentDomainAssembly(eventTypeName);

            if (type == null)
                return null;

            Debug.Assert(type.BaseType == typeof(T), "type from event differs from the type the app expects");

            typeNameMap.TryAdd(type, eventTypeName);

            return type;
        });

    private static Type? GetFirstMatchingTypeFromCurrentDomainAssembly(string typeName) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
            .FirstOrDefault();
}
