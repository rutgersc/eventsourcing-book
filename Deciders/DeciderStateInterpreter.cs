using System.Collections.Immutable;
using Deciders.Many;

namespace Deciders;

public class DeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr>(
    Decider<TId, TCommand, TEvent, TState, TErr> decider,
    IDeciderStatePersistence<TId, TState> persistence)
    where TId : notnull
    where TErr : notnull
{
    public DeciderStateInterpreter(
        Decider<TId, TCommand, TEvent, TState, TErr> decider,
        Func<TId, TState, Task<TState>> loadState,
        Func<TId, TState, Task<TId>> saveState)
        : this(decider, new DeciderStatePersistence<TId, TState>(loadState, saveState))
    {
    }

    public async Task<TStateView> SubscribeStateView<TStateView>(TId id, StateView<TEvent, TStateView> stateView)
    {
        var state = await persistence.LoadState(id, decider.InitialState);
        var readModel = new TEvent[] {}.Aggregate(stateView.InitialState, stateView.Evolve);
        return readModel;
    }

    public async Task<Result<IReadOnlyCollection<TEvent>, TErr>> ExecuteCommand(TId id, TCommand command)
    {
        var state = await persistence.LoadState(id, decider.InitialState);

        var result = decider.StateChange(existingEvents: [], existingState: state, command: command);

        if (result.TryPickError(out var error, out var output))
        {
            return error;
        }

        await persistence.SaveState(id, output.NewState);

        return new Result<IReadOnlyCollection<TEvent>, TErr>.Ok(output.NewEvents);
    }

    public async Task<Result<StateWithEvents<TState, TEvent>, TErr>> ExecuteCommands(TId id, params TCommand[] commands)
    {
        var state = await persistence.LoadState(id, decider.InitialState);

        var newEvents = ImmutableList<TEvent>.Empty;

        foreach (var command in commands)
        {
            var result = decider.StateChange(existingEvents: [], existingState: state, command: command);

            if (result.TryPickError(out var error, out var newEventsAndState))
            {
                return error;
            }

            newEvents = newEvents.AddRange(newEventsAndState.NewEvents);
            state = newEventsAndState.NewState;
        }

        // await persistence.SaveState(id, state, newEvents);

        return new Result<StateWithEvents<TState, TEvent>, TErr>.Ok(new StateWithEvents<TState, TEvent>(state, newEvents));
    }
}

public static partial class DeciderExtensions
{
    public static DeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr> ConfigureState<TId, TCommand, TEvent, TState, TErr>(
        this Decider<TId, TCommand, TEvent, TState, TErr> decider,
        Func<TId, TState, Task<TState>> loadState,
        Func<TId, TState, Task<TId>> saveState)
        where TId : notnull
        where TErr : notnull
    {
        return new DeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr>(decider, loadState, saveState);
    }


    public static DeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr> ConfigureState<TId, TCommand, TEvent, TState, TErr>(
        this Decider<TId, TCommand, TEvent, TState, TErr> decider,
        IDeciderStatePersistence<TId, TState> persistence)
        where TId : notnull
        where TErr : notnull
    {
        return new DeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr>(decider, persistence);
    }

    public static ManyDeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr> ConfigureState<TId, TCommand, TEvent, TState, TErr>(
        this ManyDecider<TId, TCommand, TEvent, TState, TErr> decider,
        IManyDeciderStatePersistence<TId, TState> persistence)
        where TId : notnull
        where TErr : notnull
    {
        return new ManyDeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr>(decider, persistence);
    }
}
