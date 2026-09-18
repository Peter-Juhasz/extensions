namespace System.IO;

public static class Int64Extensions
{
	private static readonly string[] ByteUnits = ["bytes", "KB", "MB", "GB", "TB", "PB", "EB"];

	extension(long value)
	{
		public string ToFileSize()
		{
			int num = 0;
			while (value >= 1024)
			{
				value /= 1024;
				num++;
			}

			return $"{value:F2} {ByteUnits[num]}";
		}
	}
}
