using SocialMediaApp.Models.Shared;
using System.Net;

namespace SocialMediaApp.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (StatusException statusException)
            {
                context.Response.StatusCode = (int)statusException.StatusCode;
                await context.Response.WriteAsJsonAsync<ApiResponse<string>>(new ApiResponse<string>(statusException.StatusCode, statusException.Message));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync<ApiResponse<string>>(new ApiResponse<string>(HttpStatusCode.InternalServerError, ex.Message, false));
            }
        }
    }
}
