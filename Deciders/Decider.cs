using System.Collections.Immutable;

namespace Deciders;

public record Decider<TId, TCommand, TEvent, TState, TErr>(
    Func<TCommand, TState, Result<IReadOnlyCollection<TEvent>, TErr>> Decide,
    Func<TState, TEvent, TState> Evolve,
    TState InitialState)
    where TId : notnull
    where TErr : notnull
{
    public Result<(IReadOnlyCollection<TEvent> NewEvents, TState NewState), TErr> StateChange(
        IReadOnlyCollection<TEvent> existingEvents,
        TState existingState,
        TCommand command)
    {
        var currentState = existingEvents.Aggregate(existingState, this.Evolve);

        if (this.Decide(command, currentState).TryPickError(out var error, out var newEvents))
        {
            return error;
        }

        var newState = newEvents.Aggregate(currentState, this.Evolve);

        return (newEvents, newState);
    }

    public Result<(IReadOnlyCollection<TEvent> NewEvents, TState NewState), TErr> StateChanges(
        IReadOnlyCollection<TEvent> existingEvents,
        TState existingState,
        IEnumerable<TCommand> commands)
    {
        var currentState = existingEvents.Aggregate(existingState, this.Evolve);
        var allEvents = ImmutableList<TEvent>.Empty;

        foreach (var command in commands)
        {
            var result = this.Decide(command, currentState);

            if (result.TryPickError(out var error, out var newEvents))
            {
                return error;
            }

            allEvents = allEvents.AddRange(newEvents);
            currentState = newEvents.Aggregate(currentState, this.Evolve);
        }

        return (allEvents, currentState);
    }
}
