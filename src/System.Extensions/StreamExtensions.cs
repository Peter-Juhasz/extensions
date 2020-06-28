using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamExtensions
    {
        public static Stream Rewind(this Stream stream)
        {
            stream.Position = 0L;
            return stream;
        }

        public static async Task<byte[]> ToByteArrayAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stream is MemoryStream ms)
                return ms.ToArray();

            byte[] buffer = new byte[stream.Length];
            using (MemoryStream ms2 = new MemoryStream(buffer))
            {
                await stream.Rewind().CopyToAsync(ms2, cancellationToken);
            }

            return buffer;
        }

        public static async Task<MemoryStream> BufferAsync(this Stream stream, int bufferSize = 81920, CancellationToken cancellationToken = default)
        {
            if (stream is MemoryStream memoryStream)
                return memoryStream;

            var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, bufferSize, cancellationToken);
            return (buffer.Rewind() as MemoryStream)!;
        }

        public static async IAsyncEnumerable<string> EnumerateLinesAsync(this TextReader reader)
        {
            var line = await reader.ReadLineAsync();
            while (line != null)
            {
                yield return line;
                line = await reader.ReadLineAsync();
            }
        }
    }
}
