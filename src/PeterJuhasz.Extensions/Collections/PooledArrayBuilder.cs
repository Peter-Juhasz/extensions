using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

public ref struct PooledArrayBuilder<T> : IWriteOnlyCollection<T>, IDisposable
{
	public PooledArrayBuilder()
	{
		Unsafe.SkipInit(out _element0);
		Unsafe.SkipInit(out _element1);
		Unsafe.SkipInit(out _element2);
	}

	private PooledObject<ImmutableArray<T>.Builder>? _pooledBuilder;

	private const int InlineCapacity = 3;
	private T _element0;
	private T _element1;
	private T _element2;

	private int _count = 0;

	public void Add(T item)
	{
		if (_count < InlineCapacity)
		{
			switch (_count++)
			{
				case 0: _element0 = item; break;
				case 1: _element1 = item; break;
				case 2: _element2 = item; break;
			}
			return;
		}

		if (_pooledBuilder is not { Object: { } builder })
		{
			_pooledBuilder = ImmutableArrayBuilderPool<T>.GetPooledObject(out builder);
			builder.AddRange(_element0, _element1, _element2);
		}

		builder.Add(item);
		_count++;
	}

	public void AddRange(ReadOnlySpan<T> values)
	{
		if (values.IsEmpty)
		{
			return;
		}

		if (_pooledBuilder is { Object: { } builder })
		{
			builder.AddRange(values);
			_count += values.Length;
			return;
		}

		foreach (var value in values)
		{
			Add(value);
		}
	}

	public readonly bool Any() => Count > 0;

	public readonly bool Any(Func<T, bool> predicate)
	{
		if (_pooledBuilder is { Object: { } builder })
		{
			return builder.Any(predicate);
		}

		foreach (var i in this)
		{
			if (predicate(i))
			{
				return true;
			}
		}

		return false;
	}

	public readonly bool Contains(T item, IEqualityComparer<T>? comparer = null)
	{
		if (_pooledBuilder is { Object: { } builder })
		{
			return builder.Contains(item, comparer);
		}

		comparer ??= EqualityComparer<T>.Default;
		foreach (var i in this)
		{
			if (comparer.Equals(i, item))
			{
				return true;
			}
		}

		return false;
	}

	public T this[int index]
	{
		readonly get
		{
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _count);

			if (_pooledBuilder is { } builder)
			{
				return builder.Object[index];
			}

			return index switch
			{
				0 => _element0,
				1 => _element1,
				2 => _element2,
				_ => throw new ArgumentOutOfRangeException(nameof(index), "Index out of range."),
			};
		}
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _count);

			if (_pooledBuilder is { Object: { } builder })
			{
				builder[index] = value;
				return;
			}

			switch (index)
			{
				case 0: _element0 = value; break;
				case 1: _element1 = value; break;
				case 2: _element2 = value; break;
				default:
					throw new ArgumentOutOfRangeException(nameof(index), "Index out of range.");
			}
		}
	}

	public readonly int Count => _count;

	public void Clear()
	{
		if (_pooledBuilder is { } builder)
		{
			builder.Dispose();
			_pooledBuilder = null;
		}

		_count = 0;
		_element0 = default!;
		_element1 = default!;
		_element2 = default!;
	}

	public readonly T[] ToArray()
	{
		if (_pooledBuilder is { Object: { } builder })
		{
			return builder.ToArray();
		}

		return _count switch
		{
			1 => [_element0],
			2 => [_element0, _element1],
			3 => [_element0, _element1, _element2],
			0 => [],
			_ => throw new InvalidOperationException("Invalid count."),
		};
	}

	public readonly T[]? ToArrayOrNull()
	{
		if (!Any())
		{
			return null;
		}

		return ToArray();
	}

	public readonly ImmutableArray<T> ToImmutableArray()
	{
		if (_count == 0)
		{
			return [];
		}

		if (_pooledBuilder is { Object: { } builder })
		{
			return builder.ToImmutableArray();
		}

		var array = ToArray();
		return ImmutableCollectionsMarshal.AsImmutableArray(array);
	}

	public readonly List<T> ToMutableList()
	{
		var list = new List<T>(Count);

		if (Count <= InlineCapacity)
		{
			foreach (var item in this)
			{
				list.Add(item);
			}
		}
		else
		{
			list.AddRange(_pooledBuilder!.Value.Object);
		}

		return list;
	}

	public readonly Enumerator GetEnumerator() => new(this);

	public void Dispose()
	{
		Clear();
	}


	public ref struct Enumerator(PooledArrayBuilder<T> builder) : IEnumerator<T>
	{
		private readonly PooledArrayBuilder<T> _builder = builder;
		private int _index = -1;

		public readonly T Current => _builder[_index];

		readonly object? System.Collections.IEnumerator.Current => Current;

		public bool MoveNext()
		{
			_index++;
			return _index < _builder.Count;
		}

		public void Reset() => _index = -1;

		public readonly void Dispose() { }
	}
}

public static partial class Extensions
{
	public static StringValues ToStringValues(this PooledArrayBuilder<string> builder) => builder.Count switch
	{
		0 => new StringValues((string?)null),
		1 => builder[0],
		_ => new StringValues(builder.ToArray()),
	};

	public static decimal Min(this PooledArrayBuilder<decimal> builder)
	{
		if (!builder.Any())
		{
			throw new InvalidOperationException("Sequence contains no elements");
		}

		var min = decimal.MaxValue;
		foreach (var value in builder)
		{
			if (value < min)
			{
				min = value;
			}
		}
		return min;
	}

	public static decimal Max(this PooledArrayBuilder<decimal> builder)
	{
		if (!builder.Any())
		{
			throw new InvalidOperationException("Sequence contains no elements");
		}
		var max = decimal.MinValue;
		foreach (var value in builder)
		{
			if (value > max)
			{
				max = value;
			}
		}
		return max;
	}

	public static string Concat(this PooledArrayBuilder<string> builder)
	{
		if (!builder.Any())
		{
			return string.Empty;
		}
		if (builder.Count == 1)
		{
			return builder[0];
		}
		var totalLength = 0;
		for (int i = 0; i < builder.Count; i++)
		{
			totalLength += builder[i].Length;
		}
		return string.Create(totalLength, builder, (span, state) =>
		{
			var builder = state;
			var index = 0;
			for (int i = 0; i < builder.Count; i++)
			{
				builder[i].CopyTo(span[index..]);
				index += builder[i].Length;
			}
		});
	}

	public static string Join(this PooledArrayBuilder<string> builder, char separator)
	{
		if (!builder.Any())
		{
			return string.Empty;
		}

		if (builder.Count == 1)
		{
			return builder[0];
		}

		var totalLength = 0;
		for (int i = 0; i < builder.Count; i++)
		{
			totalLength += builder[i].Length;
		}

		totalLength += builder.Count - 1;

		return string.Create(totalLength, new JoinState(builder, separator), (span, state) =>
		{
			var separator = state.separator;
			var builder = state.builder;
			var index = 0;
			for (int i = 0; i < builder.Count; i++)
			{
				if (i > 0)
				{
					span[index++] = state.separator;
				}
				builder[i].CopyTo(span[index..]);
				index += builder[i].Length;
			}
		});
	}

	private readonly ref struct JoinState(PooledArrayBuilder<string> builder, char separator)
	{
		public readonly PooledArrayBuilder<string> builder = builder;
		public readonly char separator = separator;
	}
}