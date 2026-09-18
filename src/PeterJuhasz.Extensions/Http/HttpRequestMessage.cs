namespace System.Net.Http;

public static partial class Extensions
{
	extension(HttpRequestMessage request)
	{
		public HttpRequestMessage Copy()
		{
			var copy = new HttpRequestMessage(request.Method, request.RequestUri)
			{
				Version = request.Version,
				VersionPolicy = request.VersionPolicy,
				Content = request.Content,
			};
			foreach (var header in request.Headers)
			{
				copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}
			foreach (var property in request.Options)
			{
				copy.Options.Set(new(property.Key), property.Value);
			}
			return copy;
		}
	}
}
