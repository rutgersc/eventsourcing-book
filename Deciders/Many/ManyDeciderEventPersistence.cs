namespace Deciders.Many;

public interface IManyDeciderEventPersistence<TId, TEvent>
    : IDeciderEventPersistence<
        IReadOnlyCollection<TId>,
        (TId Id, TEvent Event)>
    where TId : notnull;
