using System.Net;

namespace SocialMediaApp.Models.Shared
{
    public class StatusException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public StatusException(string message) : base(message)
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }

        public StatusException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
