namespace Deciders;

public record StateView<TEvent, TState>(
    Func<TState, TEvent, TState> Evolve,
    TState InitialState)
{
    public TState Create(IReadOnlyCollection<TEvent> events) =>
        events.Aggregate(this.InitialState, this.Evolve);
}

