namespace System;

public sealed class ReadOnlyMemoryStream(ReadOnlyMemory<byte> memory) : Stream
{
	private long _position;

	public override bool CanRead => true;
	public override bool CanSeek => true;
	public override bool CanWrite => false;

	public override long Length => memory.Length;

	public override long Position
	{
		get => _position;
		set
		{
			if (value < 0 || value > Length)
				throw new ArgumentOutOfRangeException(nameof(value));
			_position = value;
		}
	}

	public override void Flush() { }

	public override int Read(byte[] buffer, int offset, int count)
	{
		ArgumentNullException.ThrowIfNull(buffer);
		if ((uint)offset > (uint)buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(offset));
		if ((uint)count > (uint)(buffer.Length - offset))
			throw new ArgumentOutOfRangeException(nameof(count));
		return Read(buffer.AsSpan(offset, count));
	}

	public override int Read(Span<byte> destination)
	{
		int available = (int)Math.Min(destination.Length, Length - _position);
		if (available <= 0)
			return 0;

		memory.Span.Slice((int)_position, available).CopyTo(destination);
		_position += available;
		return available;
	}

	public override int ReadByte()
	{
		if (_position >= Length)
			return -1;

		return memory.Span[(int)_position++];
	}

	public override ValueTask<int> ReadAsync(
		Memory<byte> buffer,
		CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
			return ValueTask.FromCanceled<int>(cancellationToken);

		try
		{
			int read = Read(buffer.Span);
			return ValueTask.FromResult(read);
		}
		catch (Exception ex)
		{
			return ValueTask.FromException<int>(ex);
		}
	}

	public override Task<int> ReadAsync(
		byte[] buffer,
		int offset,
		int count,
		CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled<int>(cancellationToken);

		try
		{
			int read = Read(buffer, offset, count);
			return Task.FromResult(read);
		}
		catch (Exception ex)
		{
			return Task.FromException<int>(ex);
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		long newPosition = origin switch
		{
			SeekOrigin.Begin => offset,
			SeekOrigin.Current => _position + offset,
			SeekOrigin.End => Length + offset,
			_ => throw new ArgumentOutOfRangeException(nameof(origin))
		};

		if (newPosition < 0 || newPosition > Length)
			throw new IOException("Attempted to seek outside the stream bounds.");

		_position = newPosition;
		return _position;
	}

	public override void SetLength(long value) =>
		throw new NotSupportedException();

	public override void Write(byte[] buffer, int offset, int count) =>
		throw new NotSupportedException();

	public override void Write(ReadOnlySpan<byte> buffer) =>
		throw new NotSupportedException();

	public override ValueTask WriteAsync(
		ReadOnlyMemory<byte> buffer,
		CancellationToken cancellationToken = default) =>
		ValueTask.FromException(new NotSupportedException());

	public override Task WriteAsync(
		byte[] buffer,
		int offset,
		int count,
		CancellationToken cancellationToken) =>
		Task.FromException(new NotSupportedException());
}
