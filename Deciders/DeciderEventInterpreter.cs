namespace Deciders;

public class DeciderEventInterpreter<TId, TCommand, TEvent, TState, TErr>(
    Decider<TId, TCommand, TEvent, TState, TErr> decider,
    IDeciderEventPersistence<TId, TEvent> persistence)
    where TId : notnull
    where TErr : notnull
{
    public IDeciderEventPersistence<TId, TEvent> Persistence => persistence;

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

        var result = decider.StateChange(existingEvents: existingEvents, existingState: decider.InitialState, command: command);

        if (result.TryPickError(out var error, out var output))
        {
            return error;
        }

        await persistence.SaveEvents(id, output.NewEvents);

        return new Result<IReadOnlyCollection<TEvent>, TErr>.Ok(output.NewEvents);
    }
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
