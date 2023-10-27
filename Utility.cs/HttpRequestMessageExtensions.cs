using System.Text.Json;

namespace Utility.cs
{
    public static class HttpRequestMessageExtensions
    {
        public static T? GetRequestEntity<T>(this HttpRequestMessage request)
        {
            var requestContent = request.Content;
            var jsonContent = requestContent?.ReadAsStringAsync().Result;
            if (jsonContent != null) return JsonSerializer.Deserialize<T>(jsonContent);
            return default(T);
        }
    }
}
