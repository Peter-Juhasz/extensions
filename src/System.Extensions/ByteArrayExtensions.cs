using System.IO;

namespace System
{
    public static class ByteArrayExtensions
    {
        public static string ToBase64String(this ReadOnlySpan<byte> span) => Convert.ToBase64String(span);

        public static MemoryStream ToMemoryStream(this byte[] str) => new MemoryStream(str);
    }
}
