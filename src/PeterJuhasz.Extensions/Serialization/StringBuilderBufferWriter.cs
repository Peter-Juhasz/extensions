using System.Buffers;

namespace System.Text;

public readonly struct StringBuilderBufferWriter(StringBuilder builder) : IBufferWriter<char>
{
	public void Write(char value)
	{
		builder.Append(value);
	}

	public void Write(ReadOnlySpan<char> values)
	{
		builder.Append(values);
	}

	public readonly Span<char> GetSpan(int minimumLength)
	{
		throw new NotSupportedException();
	}

	public readonly Memory<char> GetMemory(int minimumLength)
	{
		throw new NotSupportedException();
	}

	public void Advance(int count)
	{
		throw new NotImplementedException();
	}
}
