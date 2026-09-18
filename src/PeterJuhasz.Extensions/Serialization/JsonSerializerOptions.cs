using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json;

public static partial class Extensions
{
	extension(JsonSerializerOptions jsonSerializerOptions)
	{
		[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
		public JsonTypeInfo<T> GetTypeInfo<T>() => (JsonTypeInfo<T>)jsonSerializerOptions.GetTypeInfo(typeof(T));
	}

	extension(JsonSerializerContext jsonSerializerContext)
	{
		[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
		public JsonTypeInfo<T> GetTypeInfo<T>() => (JsonTypeInfo<T>)(jsonSerializerContext.GetTypeInfo(typeof(T)) ?? throw new InvalidOperationException($"Type {typeof(T)} is not supported by this JsonSerializerContext."));
	}
}
