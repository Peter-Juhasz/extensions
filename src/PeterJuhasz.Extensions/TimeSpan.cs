namespace System;

public static partial class Extensions
{
	extension(TimeSpan timeSpan)
	{
		public TimeSpan AddMinutes(double minutes) => timeSpan + TimeSpan.FromMinutes(minutes);

		public TimeSpan AddSeconds(double seconds) => timeSpan + TimeSpan.FromSeconds(seconds);
	}
}
