using System.Net;

namespace System
{
    public static class UriExtensions
    {
        public static Uri AppendQueryString(this Uri uri, string name, string? value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (value == null)
                return uri;
            
            var builder = new UriBuilder(uri);
            var query = builder.Query;
            if (query.Length > 1)
            {
                query += "&";
            }
            builder.Query = query + WebUtility.UrlEncode(name) + "=" + WebUtility.UrlEncode(value);
            return builder.Uri;
        }

        public static Uri Append(this Uri uri, Uri relativeUri) => new Uri(uri, relativeUri);

        public static bool IsSchemeRelative(this Uri uri) => uri.ToString().StartsWith("//", StringComparison.Ordinal);
    }
}
