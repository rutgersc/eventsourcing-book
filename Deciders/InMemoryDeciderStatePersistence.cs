// using System.Collections.Immutable;
//
// namespace Deciders;
//
// public class InMemoryDeciderStatePersistence<TId, TState, TEvent>
//     : Dictionary<TId, TState>,
//       IDeciderStatePersistence<TId, TState, TEvent>,
//       IManyDeciderStatePersistence<TId, TState, TEvent>
//     where TId : notnull
// {
//     public Task<TState> LoadState(TId id, TState initialState)
//     {
//         return Task.FromResult(this.GetValueOrDefault(id) ?? initialState);
//     }
//
//     public Task<TId> SaveState(TId id, TState state, IReadOnlyCollection<TEvent> events)
//     {
//         this[id] = state;
//
//         return Task.FromResult(id);
//     }
//
//     public Task<ImmutableDictionary<TId, TState>> LoadState(IReadOnlyCollection<TId> ids, ImmutableDictionary<TId, TState> initialState)
//     {
//         var items = ids
//             .Select(id => new KeyValuePair<TId, TState>(id, this.GetValueOrDefault(id)!))
//             .Where(kvp => kvp.Value != null)
//             .ToImmutableDictionary();
//
//         return Task.FromResult(initialState.AddRange(items));
//     }
//
//     public async Task<IReadOnlyCollection<TId>> SaveState(
//         IReadOnlyCollection<TId> _, // for a 'many' decider, the ids are embedded in the state
//         ImmutableDictionary<TId, TState> states,
//         IReadOnlyCollection<(TId Id, TEvent Event)> events)
//     {
//         var ids = new List<TId>(states.Count);
//         foreach (var state in states)
//         {
//             var e = events
//                 .Where(e => e.Id.Equals(state.Key))
//                 .Select(e => e.Event)
//                 .ToList();
//
//              var id = await SaveState(state.Key, state.Value, e);
//              ids.Add(id);
//         }
//
//         return ids.ToImmutableList();
//     }
// }
