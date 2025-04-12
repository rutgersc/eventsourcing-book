using System.Collections.Immutable;

namespace Deciders.Many;

public class ManyDeciderEventInterpreter<TId, TCommand, TEvent, TState, TErr>(
    ManyDecider<TId, TCommand, TEvent, TState, TErr> decider,
    IManyDeciderEventPersistence<TId, TEvent> deciderPersistence)
    : DeciderEventInterpreter<
        IReadOnlyCollection<TId>,
        (TId Id, TCommand Command),
        (TId Id, TEvent Event),
        ImmutableDictionary<TId, TState>,
        (TId Id, TErr Error)>(decider, deciderPersistence)
    where TId : notnull
    where TErr : notnull;
