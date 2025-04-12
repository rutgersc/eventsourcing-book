namespace Deciders;

public class DeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr>(
    Decider<TId, TCommand, TEvent, TState, TErr> decider,
    IDeciderStatePersistence<TId, TState> persistence)
    where TId : notnull
    where TErr : notnull
{
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
}

public static partial class DeciderExtensions
{
    public static DeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr> ConfigureState<TId, TCommand, TEvent, TState, TErr>(
        this Decider<TId, TCommand, TEvent, TState, TErr> decider,
        IDeciderStatePersistence<TId, TState> persistence)
        where TId : notnull
        where TErr : notnull
    {
        return new DeciderStateInterpreter<TId, TCommand, TEvent, TState, TErr>(decider, persistence);
    }
}
