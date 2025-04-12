using System.Collections.Immutable;

namespace Deciders;

public class DeciderEventInterpreter<TId, TCommand, TEvent, TState, TErr>(
    Decider<TId, TCommand, TEvent, TState, TErr> decider,
    IDeciderEventPersistence<TId, TEvent> persistence)
    where TId : notnull
    where TErr : notnull
{
    // public DeciderEventInterpreter(
    //     Decider<TId, TCommand, TEvent, TState, TErr> decider,
    //     Func<TId, TState, Task<TState>> loadState,
    //     Func<TId, TState, Task<TId>> saveState)
    //     : this(decider, new DeciderStatePersistence<TId, TState, TEvent>(loadState, saveState))
    // {
    // }

    public async Task<TStateView?> ReadStateView<TStateView>(TId id, StateView<TEvent, TStateView> stateView)
    {
        var events = await persistence.LoadEvents(id);

        if (events == null)
        {
            return default;
        }

        var state = events.Aggregate(stateView.InitialState, stateView.Evolve);
        return state;
    }

    public async Task<Result<IReadOnlyCollection<TEvent>, TErr>> ExecuteCommand(TId id, TCommand command)
    {
        var existingEvents = await persistence.LoadEvents(id) ?? [];
        var existingState = decider.InitialState;

        var currentState = existingEvents.Aggregate(existingState, decider.Evolve);

        var result = decider.Decide(command, currentState);
        if (result.TryPickError(out var error, out var newEvents))
        {
            return error;
        }

        var newState = newEvents.Aggregate(currentState, decider.Evolve);

        _ = newState; // no need to persist since all state is derived from events
        await persistence.SaveEvents(id, newEvents);

        return new Result<IReadOnlyCollection<TEvent>, TErr>.Ok(existingEvents);
    }

    // public async Task<Result<StateWithEvents<TState, TEvent>, TErr>> ExecuteCommands(TId id, params TCommand[] commands)
    // {
    //     var state = await persistence.LoadState(id, decider.InitialState);
    //
    //     var newEvents = ImmutableList<TEvent>.Empty;
    //
    //     foreach (var command in commands)
    //     {
    //         var result = decider.Decide(command, state);
    //
    //         if (result.TryPickError(out var error, out var events))
    //         {
    //             return error;
    //         }
    //
    //         newEvents = newEvents.AddRange(events);
    //         state = events.Aggregate(state, decider.Evolve);
    //     }
    //
    //     // await persistence.SaveState(id, state, newEvents);
    //
    //     return new Result<StateWithEvents<TState, TEvent>, TErr>.Ok(new StateWithEvents<TState, TEvent>(state, newEvents));
    // }
}

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
    where TErr : notnull
{
}


public static partial class DeciderExtensions
{
    public static DeciderEventInterpreter<TId, TCommand, TEvent, TState, TErr> ConfigureEventPersistence<TId, TCommand, TEvent, TState, TErr>(
        this Decider<TId, TCommand, TEvent, TState, TErr> decider,
        IDeciderEventPersistence<TId, TEvent> persistence)
        where TId : notnull
        where TErr : notnull
    {
        return new DeciderEventInterpreter<TId, TCommand, TEvent, TState, TErr>(decider, persistence);
    }
}
