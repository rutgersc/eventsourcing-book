using System.Collections.Immutable;

namespace Deciders;

public record DeciderStatePersistence<TId, TState>(
    Func<TId, TState, Task<TState>> LoadState,
    Func<TId, TState, Task<TId>> SaveState) : IDeciderStatePersistence<TId, TState>
    where TId : notnull
{
    Task<TState> IDeciderStatePersistence<TId, TState>.LoadState(TId id, TState initialState) =>
        LoadState(id, initialState);

    Task<TId> IDeciderStatePersistence<TId, TState>.SaveState(TId id, TState state) =>
        SaveState(id, state);
}

public record ManyDeciderStatePersistence<TId, TState, TEvent>(
    Func<IReadOnlyCollection<TId>, Task<ImmutableDictionary<TId, TState>>> LoadState,
    Func<ImmutableList<StateWithEventsAndId<TId, TState, TEvent>>, Task<ImmutableList<TId>>> SaveState)
    where TId : notnull;
