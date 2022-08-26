using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace MvcCoreWeb.Tools
{
    /// <summary>
    /// 权限承载实体
    /// </summary>
    public class PolicyRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 无权限 Action
        /// </summary>
        public string DeniedAction { get; set; }

        /// <summary>
        /// 登录 Action
        /// </summary>
        public string LoginAction { get; set; }

        public PolicyRequirement()
        {
            //没有权限则跳转到这个路由
            DeniedAction = new PathString("/api/Auth/NoPermission");
            // 登录凭据过期或未登录
            LoginAction = new PathString("/api/Auth/LoginPage");
        }
    }
}
