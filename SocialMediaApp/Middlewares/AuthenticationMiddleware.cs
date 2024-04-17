using Microsoft.VisualBasic;

namespace SocialMediaApp.Middlewares
{
    public class AuthenticationMiddleware(RequestDelegate _next)
    {
        public async Task Invoke(HttpContext context)
        {
            var accessToken = string.Empty;
            if (context.Request.Cookies.TryGetValue(Constants.ACCESS_TOKEN, out accessToken))
            {
                context.Request.Headers.Authorization = $"Bearer {accessToken}";
            }
            await _next(context);
        }
    }
}
