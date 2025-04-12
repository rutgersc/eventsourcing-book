using System.Collections.Immutable;

namespace Deciders;

public interface IDeciderStatePersistence<TId, TState>
{
    Task<TState> LoadState(TId id, TState initialState);

    Task<TId> SaveState(TId id, TState state);
}

public interface IDeciderEventPersistence<TId, TEvent>
{
    Task<IReadOnlyCollection<TEvent>?> LoadEvents(TId id);

    Task SaveEvents(TId id, IReadOnlyCollection<TEvent> events);
}

public interface IManyDeciderStatePersistence<TId, TState>
    : IDeciderStatePersistence<
        IReadOnlyCollection<TId>,
        ImmutableDictionary<TId, TState>>
    where TId : notnull
{
}

public interface IManyDeciderEventPersistence<TId, TEvent>
    : IDeciderEventPersistence<
        IReadOnlyCollection<TId>,
        (TId Id, TEvent Event)>
    where TId : notnull
{
}
