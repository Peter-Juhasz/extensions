using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json;

namespace System.Net.Http;

public static partial class HttpClientExtensions
{
	extension(HttpContentHeaders headers)
	{
		/// <summary>
		/// Gets the media type from the `Content-Type` header and handles malformed header values.
		/// </summary>
		public string? GetMediaType()
		{
			if (headers.TryGetValues("Content-Type", out var contentTypeHeader) &&
				MediaTypeHeaderValue.TryParse(contentTypeHeader.First().TrimEnd(';'), out var contentType)
			)
			{
				return contentType.MediaType?.ToLowerInvariant();
			}

			return null;
		}
	}

	extension(HttpClient http)
	{
		public async Task<BinaryData> GetBinaryDataAsync(string uri, CancellationToken cancellationToken)
		{
			using var response = await http.GetAsync(uri, cancellationToken);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsBinaryDataAsync(cancellationToken);
		}

		public async Task<BinaryData> GetBinaryDataAsync(Uri uri, CancellationToken cancellationToken)
		{
			using var response = await http.GetAsync(uri, cancellationToken);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsBinaryDataAsync(cancellationToken);
		}

		[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
		public async Task<JsonDocument> GetJsonAsync(Uri uri, CancellationToken cancellationToken = default)
		{
			using var response = await http.GetAsync(uri, cancellationToken);
			response.EnsureSuccessStatusCode();
			await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
		}
	}

	extension(HttpContent http)
	{
		public async Task<BinaryData> ReadAsBinaryDataAsync(CancellationToken cancellationToken)
		{
			// fast path for binary data
			if (http is BinaryDataContent { Data: { } data })
			{
				return data;
			}

			var stream = await http.ReadAsStreamAsync(cancellationToken);
			if (stream.CanSeek)
			{
				stream.Seek(0, SeekOrigin.Begin);
			}
			data = await BinaryData.FromStreamAsync(stream, http.Headers.GetMediaType(), cancellationToken);
			if (stream.CanSeek)
			{
				stream.Seek(0, SeekOrigin.Begin);
			}
			return data;
		}
	}

	extension(string html)
	{
		public string DecodeHtml() => WebUtility.HtmlDecode(html);

		public string DecodeUri() => Uri.UnescapeDataString(html);
	}

	extension(ReadOnlySpan<char> html)
	{
		public string DecodeUri() => Uri.UnescapeDataString(html);
	}
}