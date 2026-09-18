namespace System.IO;

public static class PathExtensions
{
	public static string FindAncestor(string path, string parentFolderName)
	{
		var directoryInfo = new DirectoryInfo(path);
		while (directoryInfo != null)
		{
			if (string.Equals(directoryInfo.Name, parentFolderName, StringComparison.OrdinalIgnoreCase))
			{
				return directoryInfo.FullName;
			}
			directoryInfo = directoryInfo.Parent;
		}

		throw new ArgumentException($"Parent folder '{parentFolderName}' not found in path '{path}'.", nameof(path));
	}
}
