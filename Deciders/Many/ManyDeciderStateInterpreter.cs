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
    /// Invoke multiple commands on a 'many' decider.
    /// Since the `TCommand` of a 'many' decider is a tuple of (Id, Command), having to supply the ids
    /// in both the commands as well the ids parameter is redundant. This method takes away this redundancy by taking the
    /// ids of the commands and passing them to the 'ExecuteCommands(id, commands)' method of the base class.
    /// </summary>
    public async Task<Result<ImmutableDictionary<TId, IReadOnlyCollection<TEvent>>, TErr>> ExecuteCommands(IReadOnlyCollection<(TId Id, TCommand Command)> commands)
    {
        throw new NotImplementedException();
    }
}
