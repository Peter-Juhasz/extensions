namespace System.Buffers;

public static partial class Extensions
{
	public static void Write<TWriter>(this TWriter writer, byte value) where TWriter : IBufferWriter<byte>
	{
		var span = writer.GetSpan(1);
		span[0] = value;
		writer.Advance(1);
	}

	public static void Write<TWriter>(this TWriter writer, char value) where TWriter : IBufferWriter<char>
	{
		var span = writer.GetSpan(1);
		span[0] = value;
		writer.Advance(1);
	}
}
