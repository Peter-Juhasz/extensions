namespace System.Reflection;

public static partial class Extensions
{
	extension(Type type)
	{
		public bool HasCustomAttribute<T>(bool inherit = true) where T : Attribute =>
			type.GetCustomAttribute<T>(inherit) != null;
	}
}