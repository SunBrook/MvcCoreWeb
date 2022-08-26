using Microsoft.AspNetCore.Http;

namespace MvcCoreWeb.Tools
{
    public class MyHttpContext
    {
        private static IHttpContextAccessor _contextAccessor;

        public static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public static HttpContext Context => _contextAccessor.HttpContext;
    }
}
