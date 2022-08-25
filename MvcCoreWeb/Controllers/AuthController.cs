using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MvcCoreWeb.Models.DbModels;
using MvcCoreWeb.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MvcCoreWeb.Controllers
{
    /// <summary>
    /// Jwt 权限验证
    /// </summary>
    //[Route("api/[controller]")] // api/Auth 默认根据请求方式 get、post 等区分进入哪个方法
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController
    {
        private readonly SqlDbContext _context;
        public AuthController(SqlDbContext sqlDbContext)
        {
            _context = sqlDbContext;
        }

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get(string userName, string password)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                using (_context)
                {
                    var userInfo = _context.Account.FirstOrDefault(c => c.UserName == userName && c.Password == password);
                    if (userInfo == null)
                    {
                        return new NotFoundResult();
                    }

                    var issUser = JwtConfig.Instance.Issuer; // 颁发者
                    var audience = JwtConfig.Instance.Audience; // 允许哪些客户端使用
                    var secretKey = JwtConfig.Instance.SecretKey; // 密钥

                    var claims = new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Iss,issUser),
                        new Claim(JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                        new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddHours(1)).ToUnixTimeSeconds()}"),
                        new Claim(ClaimTypes.PrimarySid, userInfo.Id.ToString()),
                        new Claim(ClaimTypes.Role, "Account"),
                        new Claim(ClaimTypes.Role, "System"),
                        new Claim(ClaimTypes.Name, userName),
                        new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(new {
                            userInfo.Id,
                            Name = userName,
                            Role = new string[]{ "Account", "System" },
                            Project = "s01"
                        }))
                    };

                    // 对称密钥
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                    // 证书
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                    issuer: issUser,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds);

                    return new JsonResult(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token)
                    });
                }
            }
            else
            {
                return new BadRequestObjectResult(new { message = "username or password is incorrect." });
            }
        }

        /// <summary>
        /// 权限验证
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "Account")]
        //[Authorize(Roles = "System")]
        [Authorize(Policy = "accountSystem")]
        public IActionResult Check()
        {
            return new OkResult();
        }
    }
}
