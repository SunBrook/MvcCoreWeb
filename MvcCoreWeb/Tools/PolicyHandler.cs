using Microsoft.AspNetCore.Authorization;
using MvcCoreWeb.Models.DbModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCoreWeb.Tools
{
    public class PolicyHandler : AuthorizationHandler<PolicyRequirement>
    {
        private readonly SqlDbContext _sqlDbContext;
        public PolicyHandler(SqlDbContext sqlDbContext)
        {
            _sqlDbContext = sqlDbContext;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PolicyRequirement requirement)
        {
            // 用户权限
            List<string> userPermissions;
            // 从AuthorizationHandlerContext转成HttpContext，以便取出表头信息
            var httpContext = (context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext).HttpContext;
            // 请求 url
            var questUrl = httpContext.Request.Path.Value.ToUpperInvariant();
            // 是否经过验证
            var isAuthenticated = httpContext.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                // 获取当前用户的全部权限url
                var userPermissionUrlsRedisKey = $"user_permissionUrls_{Current.User.Id}";
                userPermissions = RedisCacheHelper.Get<List<string>>(userPermissionUrlsRedisKey);
                if (userPermissions == null)
                {
                    using (_sqlDbContext)
                    {
                        // 获取全部权限信息
                        var allPermissionsKey = $"all_permission_info";
                        var allPermissions = RedisCacheHelper.Get<List<Permission>>(allPermissionsKey);
                        if (allPermissions == null)
                        {
                            allPermissions = _sqlDbContext.Permissions.ToList();
                            RedisCacheHelper.Set(allPermissionsKey, allPermissions);
                        }

                        // 获取人员权限信息
                        var userPermissionIdsRedisKey = $"user_permissionIds_{Current.User.Id}";
                        var permissionIds = RedisCacheHelper.Get<List<int>>(userPermissionIdsRedisKey);
                        if (permissionIds == null)
                        {
                            permissionIds = _sqlDbContext.UserPermissions
                                .Where(c => c.AccountId == Current.User.Id)
                                .Select(c => c.PermissionId).ToList();
                            RedisCacheHelper.Set(userPermissionIdsRedisKey, permissionIds);
                        }

                        // 获取当前人员对应的权限信息
                        if (allPermissions.Count > 0 && permissionIds.Count > 0)
                        {
                            userPermissions = allPermissions
                                .Where(c => permissionIds.Contains(c.Id)).Select(c => c.Url.ToUpperInvariant()).ToList();
                        }
                        else
                        {
                            userPermissions = new List<string>();
                        }

                        RedisCacheHelper.Set(userPermissionUrlsRedisKey, userPermissions);
                    }
                }

                // 检查 url 是否运行访问
                if (userPermissions.Contains(questUrl))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    //无权限跳转到拒绝页面
                    httpContext.Response.Redirect(requirement.DeniedAction);
                }
            }
            else
            {
                // 跳转到登录页面
                httpContext.Response.Redirect(requirement.LoginAction);
            }
            return Task.CompletedTask;
        }
    }
}