using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlackApi.Extensions
{
    public static class HttpResponseExtensions
    {
        public static Task WriteResponseAsync(this HttpResponse httpResponse, HttpStatusCode statusCode, string message, Dictionary<string, string> modelState = null)
        {
            return httpResponse.WriteApiResponseAsync(new ApiResponse
            {
                StatusCode = (int)statusCode,
                Message = message,
                ModelState = modelState
            });
        }

        public static Task WriteApiResponseAsync(this HttpResponse httpResponse, ApiResponse apiResponse)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var serializedMessage = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(apiResponse, jsonSerializerOptions));

            return httpResponse
                .Body
                .WriteAsync(serializedMessage, 0, serializedMessage.Length);
        }
    }
}
