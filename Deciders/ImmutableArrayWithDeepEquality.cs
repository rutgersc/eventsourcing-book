using System.Collections;
using System.Collections.Immutable;

namespace Deciders;

/// <summary>
/// Class is used to compare objects by value.
/// Origin: https://gist.github.com/ryanholden8/5602a6cc6decebb5bb35e21aa37efa67
/// </summary>
/// <typeparam name="T"></typeparam>
public struct ImmutableArrayWithDeepEquality<T> : IEquatable<ImmutableArrayWithDeepEquality<T>>, IEnumerable, IEnumerable<T>
{
    private readonly ImmutableArray<T> _list;

    public ImmutableArrayWithDeepEquality(ImmutableArray<T> list) => _list = list;

    #region ImmutableArray Implementation

    public T this[int index] => _list[index];

    public int Count => _list.Length;

    public ImmutableArrayWithDeepEquality<T> Add(T value) => _list.Add(value).WithDeepEquality();
    public ImmutableArrayWithDeepEquality<T> AddRange(IEnumerable<T> items) => _list.AddRange(items).WithDeepEquality();
    public ImmutableArrayWithDeepEquality<T> Clear() => _list.Clear().WithDeepEquality();
    public ImmutableArray<T>.Enumerator GetEnumerator() => _list.GetEnumerator();
    public int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) => _list.IndexOf(item, index, count, equalityComparer);
    public ImmutableArrayWithDeepEquality<T> Insert(int index, T element) => _list.Insert(index, element).WithDeepEquality();
    public ImmutableArrayWithDeepEquality<T> InsertRange(int index, IEnumerable<T> items) => _list.InsertRange(index, items).WithDeepEquality();
    public int LastIndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) => _list.LastIndexOf(item, index, count, equalityComparer);
    public ImmutableArrayWithDeepEquality<T> Remove(T value, IEqualityComparer<T> equalityComparer) => _list.Remove(value, equalityComparer).WithDeepEquality();
    public ImmutableArrayWithDeepEquality<T> RemoveAll(Predicate<T> match) => _list.RemoveAll(match).WithDeepEquality();
    public ImmutableArrayWithDeepEquality<T> RemoveAt(int index) => _list.RemoveAt(index).WithDeepEquality();
    public ImmutableArrayWithDeepEquality<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer) => _list.RemoveRange(items, equalityComparer).WithDeepEquality();
    public ImmutableArrayWithDeepEquality<T> RemoveRange(int index, int count) => _list.RemoveRange(index, count).WithDeepEquality();
    public ImmutableArrayWithDeepEquality<T> Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer) => _list.Replace(oldValue, newValue, equalityComparer).WithDeepEquality();
    public ImmutableArrayWithDeepEquality<T> SetItem(int index, T value) => _list.SetItem(index, value).WithDeepEquality();
    public bool IsDefaultOrEmpty => _list.IsDefaultOrEmpty;

    public static ImmutableArrayWithDeepEquality<T> Empty = new(ImmutableArray<T>.Empty);

    #endregion

    #region IEnumerable

    IEnumerator IEnumerable.GetEnumerator() => (_list as IEnumerable).GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => (_list as IEnumerable<T>).GetEnumerator();

    #endregion

    #region IEquatable

    public bool Equals(ImmutableArrayWithDeepEquality<T> other) => _list.SequenceEqual(other);

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override bool Equals(object obj) => obj is ImmutableArrayWithDeepEquality<T> other && Equals(other);
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).

    public static bool operator ==(ImmutableArrayWithDeepEquality<T>? left, ImmutableArrayWithDeepEquality<T>? right) => left is null ? right is null : left.Equals(right);

    public static bool operator !=(ImmutableArrayWithDeepEquality<T>? left, ImmutableArrayWithDeepEquality<T>? right) => !(left == right);

    public override int GetHashCode()
    {
        unchecked
        {
            return _list.Aggregate(19, (h, i) => h * 19 + i!.GetHashCode());
        }
    }


    #endregion
}

public static class ImmutableArrayWithDeepEqualityEx
{
    public static ImmutableArrayWithDeepEquality<T> WithDeepEquality<T>(this ImmutableArray<T> list) => new(list);

    public static ImmutableArrayWithDeepEquality<T> ToImmutableArrayWithDeepEquality<T>(this IEnumerable<T> list) => new(list.ToImmutableArray());
}
