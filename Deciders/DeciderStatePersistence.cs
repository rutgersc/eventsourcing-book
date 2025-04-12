using System.Collections.Immutable;

namespace Deciders;

public interface IDeciderStatePersistence<TId, TState>
{
    Task<TState> LoadState(TId id, TState initialState);

    Task<TId> SaveState(TId id, TState state);
}

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

public interface IDeciderEventPersistence<TId, TEvent>
{
    Task<IReadOnlyCollection<TEvent>?> LoadEvents(TId id);

    Task SaveEvents(TId id, IReadOnlyCollection<TEvent> events);

    Task<IDisposable> Subscribe(Func<TId, TEvent, Task> onEvent);
}

