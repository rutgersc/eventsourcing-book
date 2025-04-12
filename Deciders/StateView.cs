namespace Deciders;

public record StateView<TEvent, TState>(
    Func<TState, TEvent, TState> Evolve,
    TState InitialState)
{
}
