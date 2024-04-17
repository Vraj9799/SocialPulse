using System.Net;

namespace SocialMediaApp.Models.Shared
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public T Data { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse(T data = default)
        {
            StatusCode = (int)HttpStatusCode.OK;
            Data = data;
            IsSuccess = true;
        }

        public ApiResponse(HttpStatusCode statusCode, T data = default, bool isSuccess = true)
        {
            StatusCode = (int)statusCode;
            Data = data;
            IsSuccess = isSuccess;
        }
    }
}
