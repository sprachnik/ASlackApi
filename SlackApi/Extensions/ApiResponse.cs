using System.Text.Json;

namespace SlackApi.Extensions
{
    /// <summary>
    /// Default API response.
    /// </summary>
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, string>? ModelState { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
