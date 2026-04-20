using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Models;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SqlSugar;
using System.Security.Claims;

namespace sso_test.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISqlSugarClient _db;

        // 构造函数注入 SqlSugar 客户端
        public AuthController(ISqlSugarClient db)
        {
            _db = db;
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("无法获取 OpenID Connect 请求信息。");

            if (request.IsPasswordGrantType())
            {
                var user = await _db.Queryable<Sysuser>()
                                    .FirstAsync(u => u.Username == request.Username);

                if (user == null || user.Password != request.Password)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "用户名或密码错误。"
                    });
                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // 4. 验证通过，创建身份声明 (Claims)
                var identity = new ClaimsIdentity(
                    authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, // 使用官方默认方案名
                    nameType: OpenIddictConstants.Claims.Name,
                    roleType: OpenIddictConstants.Claims.Role);

                // Subject (sub) 是必须的
                identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
                identity.AddClaim(OpenIddictConstants.Claims.Name, user.Username);
                identity.AddClaim(OpenIddictConstants.Claims.Role, "Admin");

                // 【关键修改点 1】：遍历所有声明，显式授予它们进入 AccessToken 的权限
                // 这样即使不依赖 Scope 过滤，Payload 里也一定会有数据
                foreach (var claim in identity.Claims)
                {
                    claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
                }

                var principal = new ClaimsPrincipal(identity);

                // 【关键修改点 2】：确保 Scopes 被正确处理
                // 如果客户端请求了特定的 scope，这里要通过；如果没有，这里至少要给个默认值
                var scopes = request.GetScopes();
                if (!scopes.Any())
                {
                    // 如果请求没带 scope，手动赋予基础 scope，否则有些 Claim 可能会被系统屏蔽
                    principal.SetScopes(OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.Profile);
                }
                else
                {
                    principal.SetScopes(scopes);
                }

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new { error = "unsupported_grant_type", description = "只支持密码模式。" });
        }
    }

}