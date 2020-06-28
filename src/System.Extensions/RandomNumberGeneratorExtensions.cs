using System.Security.Cryptography;

namespace System.Extensions
{
    public static class RandomNumberGeneratorExtensions
    {
        public static int GetInt32(this RandomNumberGenerator rng)
        {
            Span<byte> buffer = stackalloc byte[4];
            rng.GetBytes(buffer);
            return BitConverter.ToInt32(buffer);
        }
    }
}
