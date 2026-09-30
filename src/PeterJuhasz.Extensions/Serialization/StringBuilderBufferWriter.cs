using System.Buffers;

namespace System.Text;

public struct StringBuilderBufferWriter(StringBuilder builder) : IBufferWriter<char>
{
	private char[]? _buffer = null;

	public readonly void Write(char value)
	{
		builder.Append(value);
	}

	public readonly void Write(ReadOnlySpan<char> values)
	{
		builder.Append(values);
	}

	public Span<char> GetSpan(int minimumLength)
	{
		return GetMemory(minimumLength).Span;
	}

	public Memory<char> GetMemory(int minimumLength)
	{
		if (_buffer == null)
		{
			_buffer = new char[minimumLength];
		}

		if (_buffer.Length < minimumLength)
		{
			var oldArray = _buffer;
			var newArray = new char[minimumLength];
			oldArray.CopyTo(newArray);
			_buffer = newArray;
		}

		return _buffer;
	}

	public void Advance(int count)
	{
		if (_buffer == null)
		{
			throw new InvalidOperationException("GetMemory must be called before Advance.");
		}

		builder.Append(_buffer.AsSpan(0, count));
		_buffer.AsSpan().Clear();
	}
}
