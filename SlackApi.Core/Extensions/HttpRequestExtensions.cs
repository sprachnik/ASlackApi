using Microsoft.AspNetCore.Http;
using System.Text;

namespace SlackApi.Core.Extensions
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Retrieve the raw body as a string from the Request.Body stream
        /// </summary>
        /// <param name="request">Request instance to apply to</param>
        /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
        /// <returns></returns>
        public static string GetRawBodyString(this HttpRequest request,
            Encoding encoding = null,
            bool detectEncodingFromByteOrderMarks = false,
            int bufferSize = 512000,
            bool leaveOpen = true)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            using StreamReader reader = new(request.Body, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen);
            var result = reader.ReadToEnd();

            if (leaveOpen)
                request.Body.Position = 0;

            return result;
        }

        /// <summary>
        /// Retrieves the raw body as a byte array from the Request.Body stream
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static byte[] GetRawBodyBytes(this HttpRequest request)
        {
            using var ms = new MemoryStream(2048);
            request.Body.CopyTo(ms);
            request.Body.Position = 0;
            return ms.ToArray();
        }
    }
}
