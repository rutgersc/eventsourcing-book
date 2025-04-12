using System.Collections.Immutable;

namespace Deciders.Many;

public interface IManyDeciderStatePersistence<TId, TState>
    : IDeciderStatePersistence<
        IReadOnlyCollection<TId>,
        ImmutableDictionary<TId, TState>>
    where TId : notnull;

public record ManyDeciderStatePersistence<TId, TState, TEvent>(
    Func<IReadOnlyCollection<TId>, Task<ImmutableDictionary<TId, TState>>> LoadState,
    Func<ImmutableList<StateWithEventsAndId<TId, TState, TEvent>>, Task<ImmutableList<TId>>> SaveState)
    where TId : notnull;


