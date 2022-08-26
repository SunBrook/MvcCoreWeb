using Microsoft.AspNetCore.Http.Features;
using MvcCoreWeb.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;

namespace MvcCoreWeb.Tools
{
    public class Current
    {
        public static string ClientIp { get { return MyHttpContext.Context.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString(); } }


        public static LoginUser User
        {
            get
            {
                if (!MyHttpContext.Context.User.Identity.IsAuthenticated || !(MyHttpContext.Context.User.Identity is ClaimsIdentity identity))
                    return null;
                try
                {
                    if (int.TryParse(identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value, out int id) && id > 0)
                        return JsonConvert.DeserializeObject<LoginUser>(identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData)?.Value);
                }
                catch
                {
                    return null;
                }
                return null;
            }
        }
    }
}
