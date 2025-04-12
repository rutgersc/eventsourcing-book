using System.Collections.Immutable;

namespace Deciders;

public record StateWithEvents<TState, TEvents>(
    TState State,
    ImmutableList<TEvents> Events);

public static class StateWithEvents
{
    public static StateWithEvents<TState, TEvents> Create<TState, TEvents>(TState state, IReadOnlyCollection<TEvents> events) =>
        new(state, events.ToImmutableList());
}

public record StateWithEventsAndId<TId, TState, TEvents>(
    TId Id,
    TState State,
    ImmutableList<TEvents> Events);

public static class StateWithEventsAndId
{
    public static StateWithEventsAndId<TId, TState, TEvents> Create<TId, TState, TEvents>(TId id, TState state, IReadOnlyCollection<TEvents> events) =>
        new(id, state, events.ToImmutableList());
}
