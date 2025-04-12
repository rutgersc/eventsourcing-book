using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Deciders;
using EventStore.Client;

namespace EventsourcingBook.Infra.Common;

public class EventsRepository<TId, TEvent>(string streamPrefix, EventStoreClient client)
    : IDeciderEventPersistence<TId, TEvent>
{
    private readonly EventTypeMapping eventMapping = new();

    private string StreamName(TId id) => $"{streamPrefix}-{id}";

    private string IdFromStreamName(string streamName) => streamName[(streamPrefix.Length+1)..];

    public async Task<IReadOnlyCollection<TEvent>?> LoadEvents(TId id)
    {
        var result = client.ReadStreamAsync(
            Direction.Forwards,
            StreamName(id),
            StreamPosition.Start);

        try
        {
            var events = await result.ToListAsync();
            return events.Select(Deserialize<TEvent>).ToList();
        }
        catch (StreamNotFoundException)
        {
            return null;
        }
    }

    public async Task SaveEvents(TId id, IReadOnlyCollection<TEvent> events)
    {
        var eventData = events.Select(Serialize).ToList();

        await client.AppendToStreamAsync(
            streamName: StreamName(id),
            StreamState.Any,
            eventData: eventData);
    }

    public async Task<IDisposable> Subscribe(Func<TId, TEvent, Task> onEvent)
    {
        var subscription = await client.SubscribeToStreamAsync(
            start: FromStream.Start,
            streamName: $"$ce-{streamPrefix}",
            resolveLinkTos: true,
            eventAppeared: async (subscription, @event, cancellationToken) =>
            {
                var idString = IdFromStreamName(@event.Event.EventStreamId);
                if (!IdParser.TryParseFromStreamName<TId>(idString, out var id))
                {
                    Console.WriteLine($"id incorrect {idString}");
                    return;
                }

                var eventData = DeserializeCategory<TEvent>(@event.Event);
                await onEvent(id, eventData);
            });

        return subscription;
    }

    private EventData Serialize(TEvent @event)
    {
        return new EventData(
            eventId: Uuid.NewUuid(),
            type: eventMapping.ToName(@event.GetType()),
            data: JsonSerializer.SerializeToUtf8Bytes(@event,  @event.GetType()));
    }

    private T DeserializeCategory<T>(EventRecord resolvedEvent)
    {
        var dataType = eventMapping.ToType<T>(resolvedEvent.EventType)!;
        var jsonData = Encoding.UTF8.GetString(resolvedEvent.Data.Span);
        var data = JsonSerializer.Deserialize(jsonData, dataType);
        return (T)data!;
    }

    private T Deserialize<T>(ResolvedEvent resolvedEvent)
    {
        var dataType = eventMapping.ToType<T>(resolvedEvent.Event.EventType)!;
        var jsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
        var data = JsonSerializer.Deserialize(jsonData, dataType);
        return (T)data!;
    }

    private class EventTypeMapping
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

    public static class IdParser
    {
        public static bool TryParseFromStreamName<TId>(string idString, out TId id)
        {
            id = default;

            // Handle parsing via reflection
            var tryParseMethod = FindTryParseMethod<TId>();
            if (tryParseMethod == null) return false;

            object[] parameters = new object[] { idString, CultureInfo.InvariantCulture, null };
            bool success = (bool)tryParseMethod.Invoke(null, parameters)!;

            if (success)
            {
                id = (TId)parameters[2]!;
                return true;
            }
            return false;
        }

        private static MethodInfo? FindTryParseMethod<TId>()
        {
            return typeof(TId).GetMethod(
                "TryParse",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(string), typeof(IFormatProvider), typeof(TId).MakeByRefType() },
                null);
        }
    }

}
