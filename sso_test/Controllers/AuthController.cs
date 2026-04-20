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

            // 情况 A：密码模式
            if (request.IsPasswordGrantType())
            {
                var user = await _db.Queryable<Sysuser>().FirstAsync(u => u.Username == request.Username);

                if (user == null || user.Password != request.Password)
                {
                    return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "用户名或密码错误。"
                    }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                                                  OpenIddictConstants.Claims.Name,
                                                  OpenIddictConstants.Claims.Role);

                identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString(), OpenIddictConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIddictConstants.Claims.Name, user.Username, OpenIddictConstants.Destinations.AccessToken);

                // 保证刷新时这些 Claim 依然能进 AccessToken
                foreach (var claim in identity.Claims) claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);

                var principal = new ClaimsPrincipal(identity);
                principal.SetScopes(request.GetScopes());

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // 情况 B：刷新令牌模式
            if (request.IsRefreshTokenGrantType())
            {
                // 重点：AuthenticateAsync 会从数据库里捞出之前那个 SignIn 存进去的 Principal
                var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                if (!result.Succeeded || result.Principal == null)
                {
                    return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "刷新令牌无效。"
                    }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // 重新签发时，OpenIddict 默认会保留之前的 Claims，直接 SignIn 即可
                return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new { error = "unsupported_grant_type" });
        }
    }

}