using Microsoft.Extensions.Primitives;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System;

// - Empty: when many is null
// - One: when many is an empty array
// - Many: when many is a non-empty array

[JsonConverter(typeof(OneOrManyConverterFactory))]
public readonly struct OneOrMany<T> : IEquatable<OneOrMany<T>>
{
	public OneOrMany(T one)
	{
		_one = one;
		_many = [];
	}

	public OneOrMany(T[] many)
	{
		if (many is [var single])
		{
			_one = single;
			_many = [];
			return;
		}

		if (many.Length == 0)
		{
			_one = default;
			_many = null;
			return;
		}

		_one = default;
		_many = many;
	}

	public static readonly OneOrMany<T> Empty = default;

	private readonly T? _one;
	private readonly T[]? _many;

	public bool IsEmpty => _many == null;

	[MemberNotNullWhen(true, nameof(_one))]
	public bool HasOne
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _many?.Length == 0;
	}

	[MemberNotNullWhen(true, nameof(_many))]
	public bool HasMany
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _many?.Length > 1;
	}

	public int Count =>
		IsEmpty ? 0 :
		HasOne ? 1 :
		_many!.Length;

	public bool Any() => !IsEmpty;

	public bool Any(Func<T, bool> predicate)
	{
		if (HasOne)
		{
			return predicate(_one);
		}
		if (HasMany)
		{
			foreach (var item in _many)
			{
				if (predicate(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Contains(T value, IEqualityComparer<T> comparer)
	{
		if (HasOne)
		{
			return comparer.Equals(_one, value);
		}
		if (HasMany)
		{
			foreach (var item in _many)
			{
				if (comparer.Equals(item, value))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Contains(T value) => Contains(value, EqualityComparer<T>.Default);

	public OneOrMany<TResult> Select<TResult>(Func<T, TResult> selector)
	{
		if (HasOne)
		{
			return OneOrMany.Create(selector(_one));
		}

		if (HasMany)
		{
			var result = _many.SelectToArray(selector);
			return OneOrMany.Create(result);
		}

		return OneOrMany<TResult>.Empty;
	}

	public T First()
	{
		if (HasOne)
		{
			return _one;
		}

		if (HasMany)
		{
			return _many[0];
		}

		throw new InvalidOperationException("The collection is empty.");
	}

	public T? FirstOrDefault()
	{
		if (HasOne)
		{
			return _one;
		}

		if (HasMany)
		{
			return _many[0];
		}

		return default;
	}

	public T[] ToArray()
	{
		if (HasMany)
		{
			var copy = new T[_many.Length];
			_many.AsSpan().CopyTo(copy);
			return copy;
		}
		if (HasOne)
		{
			return [_one];
		}
		return [];
	}

	public ImmutableArray<T> ToImmutableArray()
	{
		if (HasMany)
		{
			return [.. _many];
		}
		if (HasOne)
		{
			return [_one];
		}
		return [];
	}

	public T this[int index]
	{
		get
		{
			if (HasMany)
			{
				return _many[index];
			}

			if (index == 0 && HasOne)
			{
				return _one;
			}

			throw new IndexOutOfRangeException();
		}
	}

	public void CopyTo(Span<T> span)
	{
		if (HasMany)
		{
			_many.AsSpan().CopyTo(span);
			return;
		}

		if (HasOne)
		{
			span[0] = _one;
		}
	}

	public OneOrMany<T> OrderBy<TSelector>(Func<T, TSelector> selector)
	{
		if (HasOne)
		{
			return this;
		}

		if (IsEmpty)
		{
			return this;
		}

		var sorted = new T[_many!.Length];
		_many.AsSpan().CopyTo(sorted);

		var comparer = Comparer<TSelector>.Default;
		sorted.AsSpan().Sort((x, y) => comparer.Compare(selector(x), selector(y)));

		return new(sorted);
	}



	public bool SetEqual(OneOrMany<T> other)
	{
		if (Count != other.Count) return false;
		if (IsEmpty) return true;
		if (HasOne) return EqualityComparer<T>.Default.Equals(_one!, other._one!);
		if (ReferenceEquals(_many, other._many)) return true;

		var comparer = EqualityComparer<T>.Default;
		var otherSpan = other._many!.AsSpan();
		foreach (var item in _many!)
		{
			if (otherSpan.IndexOf(item, comparer) < 0) return false;
		}
		return true;
	}

	public Enumerator GetEnumerator() => new(this);

	public IEnumerable<T> AsEnumerable()
	{
		if (HasOne)
		{
			yield return _one;
			yield break;
		}
		if (HasMany)
		{
			foreach (var item in _many)
			{
				yield return item;
			}
		}
	}

	public bool Equals(OneOrMany<T> other)
	{
		return EqualityComparer<T>.Default.Equals(this._one, other._one) && this._many == other._many;
	}

	public ref struct Enumerator(OneOrMany<T> collection)
	{
		private readonly OneOrMany<T> _collection = collection;
		private int _index = -1;

		public readonly T Current => _collection[_index];

		public bool MoveNext()
		{
			_index++;
			return _index < _collection.Count;
		}

		public void Reset()
		{
			_index = -1;
		}
	}
}

public static class OneOrMany
{
	public static OneOrMany<T> Create<T>(T one) => new(one);

	public static OneOrMany<T> CreateOrNone<T>(T? one) => one == null ? OneOrMany<T>.Empty : new(one);

	public static OneOrMany<T> Create<T>(T[] many) => new(many);

	public static OneOrMany<T> Create<T>(IEnumerable<T> oneOrMany)
	{
		using var builder = new PooledArrayBuilder<T>();
		foreach (var item in oneOrMany)
		{
			builder.Add(item);
		}

		return builder.ToOneOrMany();
	}

	public static OneOrMany<T> Empty<T>() => OneOrMany<T>.Empty;

	public static OneOrMany<T> ToOneOrMany<T>(this PooledArrayBuilder<T> builder) => builder switch
	{
		{ Count: 0 } => Empty<T>(),
		{ Count: 1 } => Create<T>(builder[0]),
		_ => Create<T>(builder.ToArray())
	};

	public static StringValues ToStringValues(this OneOrMany<string> oneOrMany)
	{
		if (oneOrMany.IsEmpty)
		{
			return StringValues.Empty;
		}
		if (oneOrMany.HasOne)
		{
			return new StringValues(oneOrMany[0]);
		}
		return new StringValues(oneOrMany.ToArray());
	}

	public static T? FirstOrNull<T>(this OneOrMany<T> oneOrMany) where T : struct
	{
		if (oneOrMany.IsEmpty)
		{
			return null;
		}

		return oneOrMany.First();
	}
}

[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
public sealed class OneOrManyConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		if (!typeToConvert.IsGenericType)
		{
			return false;
		}

		if (typeToConvert.GetGenericTypeDefinition() == typeof(OneOrMany<>))
		{
			return true;
		}

		return false;
	}

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		Type elementType = typeToConvert.GetGenericArguments()[0];
		Type converterType = typeof(OneOrManyConverter<>).MakeGenericType(elementType);
		return (JsonConverter)Activator.CreateInstance(converterType)!;
	}

	[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
	private sealed class OneOrManyConverter<T> : JsonConverter<OneOrMany<T>>
	{
		public override OneOrMany<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
		{
			JsonTokenType.StartObject => OneOrMany.Create<T>(JsonSerializer.Deserialize<T>(ref reader, options)!),
			JsonTokenType.StartArray => OneOrMany.Create<T>(JsonSerializer.Deserialize<T[]>(ref reader, options)!),
			JsonTokenType.Null => OneOrMany.Empty<T>(),
			_ => throw new JsonException($"Unexpected token {reader.TokenType} when parsing OneOrMany<{typeof(T)}>.")
		};

		public override void Write(Utf8JsonWriter writer, OneOrMany<T> value, JsonSerializerOptions options)
		{
			switch (value.Count)
			{
				case 0:
					writer.WriteNullValue();
					break;

				case 1:
					JsonSerializer.Serialize(writer, value[0], options);
					break;

				default:
					JsonSerializer.Serialize(writer, value.ToArray(), options);
					break;
			}
		}
	}
}
