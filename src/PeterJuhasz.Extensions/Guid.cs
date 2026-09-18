namespace System;

public static partial class Extensions
{
	extension(Guid value)
	{
		public Guid? NullIfEmpty() => value == Guid.Empty ? null : value;
	}
}
