namespace System.Collections.Generic;

public static class SetExtensions
{
	extension<T>(ISet<T> set)
	{
		public void AddOrRemove(T item)
		{
			if (set.Add(item))
			{
				return;
			}

			set.Remove(item);
		}
	}
}