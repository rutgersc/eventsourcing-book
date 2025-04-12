using System.Collections.Immutable;

namespace Deciders;

public record Decider<TId, TCommand, TEvent, TState, TErr>(
    Func<TCommand, TState, Result<IReadOnlyCollection<TEvent>, TErr>> Decide,
    Func<TState, TEvent, TState> Evolve,
    TState InitialState)
    where TId : notnull
    where TErr : notnull
{
    public TState EvolveState(IEnumerable<TEvent> events)
    {
        return events.Aggregate(this.InitialState, this.Evolve);
    }
};

public record ManyDecider<TId, TCommand, TEvent, TState, TErr>(
    Func<(TId Id, TCommand Command),
         ImmutableDictionary<TId, TState>,
         Result<IReadOnlyCollection<(TId Id, TEvent Event)>, (TId Id, TErr Error)>> Decide,
    Func<ImmutableDictionary<TId, TState>,
         (TId Id, TEvent Event),
         ImmutableDictionary<TId, TState>> Evolve,
    ImmutableDictionary<TId, TState> InitialState)
    : Decider<
            IReadOnlyCollection<TId>,
            (TId Id, TCommand Command),
            (TId Id, TEvent Event),
            ImmutableDictionary<TId, TState>,
            (TId Id, TErr Error)>
        (Decide, Evolve, InitialState)
    where TId : notnull;

public static class Decider
{
    public static ManyDecider<TId, TCommand, TEvent, TState, TErr> Many<TId, TCommand, TEvent, TState, TErr>(this Decider<TId, TCommand, TEvent, TState, TErr> decider)
        where TId : notnull
        where TErr : notnull
    {
        return new ManyDecider<TId, TCommand, TEvent, TState, TErr>(
            Decide: (commandWithKey, states) =>
            {
                var state = states.GetValueOrDefault(
                    key: commandWithKey.Id,
                    defaultValue: decider.InitialState);

                var decision = decider.Decide(commandWithKey.Command, state);

                if (decision.TryPickOk(out var events, out var error))
                {
                    var eventsWithId = events.Select(e => (commandWithKey.Id, e)).ToArray();
                    return eventsWithId;
                }

                return (commandWithKey.Id, error);
            },
            Evolve: (states, eventWithKey) =>
            {
                var state = states.GetValueOrDefault(
                    key: eventWithKey.Id,
                    defaultValue: decider.InitialState);

                var newState = decider.Evolve(state, eventWithKey.Event);

                return states.SetItem(eventWithKey.Id, newState);
            },
            InitialState: ImmutableDictionary<TId, TState>.Empty);
    }
}
