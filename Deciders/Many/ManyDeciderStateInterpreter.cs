using System.Collections.Immutable;

namespace Deciders.Many;

public class ManyDeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr>(
    ManyDecider<TId, TCommand, TEvent, TState, TErr> decider,
    IManyDeciderStatePersistence<TId, TState> deciderPersistence)
    : DeciderStateInterpreter<
        IReadOnlyCollection<TId>,
            (TId Id, TCommand Command),
            (TId Id, TEvent Event),
            ImmutableDictionary<TId, TState>,
            (TId Id, TErr Error)>(decider, deciderPersistence)
    where TId : notnull
    where TErr : notnull
{
    /// <summary>
    /// Invoke a single command on a 'many' decider by ignoring the fact that the decider is indexed its id.
    /// </summary>
    public async Task<Result<StateWithEvents<TState, TEvent>, TErr>> ExecuteCommand(TId id, TCommand command)
    {
        var result = await this.ExecuteCommand([id], (id, command));

        if (result.TryPickError(out var error, out var resultById))
        {
            return error.Error;
        }

        // var state = resultById.State[id];
        // var events = resultById.Events.Where(e => e.Id.Equals(id)).Select(e => e.Event).ToImmutableList();
        // return new StateWithEvents<TState, TEvent>(state, events);
        return null!;
    }

    /// <summary>
    /// Invoke multiple commands on a 'many' decider.
    /// Since the `TCommand` of a 'many' decider is a tuple of (Id, Command), having to supply the ids
    /// in both the commands as well the ids parameter is redundant. This method takes away this redundancy by taking the
    /// ids of the commands and passing them to the 'ExecuteCommands(id, commands)' method of the base class.
    /// </summary>
    public async Task<Result<ImmutableDictionary<TId, StateWithEvents<TState, TEvent>>, TErr>> ExecuteCommands(IReadOnlyCollection<(TId Id, TCommand Command)> commands)
    {
        var ids = commands.Select(c => c.Id).ToArray();
        var result = await this.ExecuteCommands(ids, commands.ToArray());

        if (result.TryPickError(out var error, out var stateWithEvents))
        {
            return error.Error;
        }

        // Take the seperate state/events that are both indexed by the id
        ImmutableDictionary<TId, TState> state = stateWithEvents.State;
        ImmutableList<(TId Id, TEvent Event)> events = stateWithEvents.Events;

        // And combine them into a single dictionary
        ImmutableDictionary<TId, StateWithEvents<TState, TEvent>> stateAndEventsById = state.ToImmutableDictionary(
            kvp => kvp.Key,
            kvp => new StateWithEvents<TState, TEvent>(
                kvp.Value,
                events.Where(e => e.Id.Equals(kvp.Key)).Select(e => e.Event).ToImmutableList()));

        return stateAndEventsById;
    }
}
