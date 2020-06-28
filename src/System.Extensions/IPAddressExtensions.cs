using System.Net.Sockets;

namespace System.Net
{
    public static class IPAddressExtensions
    {
        public static bool IsIPv4(this IPAddress ip) => !ip.IsIPv6();

        public static bool IsIPv6(this IPAddress ip) => ip.AddressFamily == AddressFamily.InterNetworkV6;
    }
}
