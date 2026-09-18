using System.Buffers;

namespace System.IO.Pipelines;

public static partial class PipeExtensions
{
	extension<T>(ReadOnlySequence<T> sequence)
	{
		public bool TryPeek(out T value)
		{
			if (sequence.IsEmpty)
			{
				value = default!;
				return false;
			}

			value = sequence.FirstSpan[0];
			return true;
		}
	}

	extension(ReadOnlySequence<byte> buffer)
	{
		public bool TryReadLine(out ReadOnlySequence<byte> line)
		{
			SequencePosition? position = buffer.PositionOf((byte)'\n');

			if (position == null)
			{
				line = default;
				return false;
			}

			line = buffer.Slice(0, buffer.GetPosition(1, position.Value));
			return true;
		}
	}

	extension(PipeReader reader)
	{
		public async ValueTask<ReadOnlySequence<byte>> ReadLineAsync(int maximumLength, CancellationToken cancellationToken)
		{
			while (true)
			{
				var result = await reader.ReadAsync(cancellationToken);
				var buffer = result.Buffer;

				if (buffer.TryReadLine(out var line))
				{
					return line;
				}
				else
				{
					reader.AdvanceTo(buffer.Start, buffer.End);
				}

				if (result.IsCompleted)
				{
					return buffer;
				}

				if (buffer.Length > maximumLength)
				{
					throw new HttpRequestException(HttpRequestError.HttpProtocolError, "Line is too long.");
				}
			}
		}
	}

	extension(PipeWriter writer)
	{
		public void WriteUtf8BOM()
		{
			var span = writer.GetSpan(3);
			span[0] = 0xEF;
			span[1] = 0xBB;
			span[2] = 0xBF;
			writer.Advance(3);
		}
	}
}