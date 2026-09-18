using System.Net.Sockets;

namespace System.Net;

public static partial class Extensions
{
	extension(IPAddress ip)
	{
		public bool IsIPv4() => !ip.IsIPv6();

		public bool IsIPv6() => ip.AddressFamily == AddressFamily.InterNetworkV6;
	}
}