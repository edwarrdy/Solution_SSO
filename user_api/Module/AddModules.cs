using OpenIddict.Validation.AspNetCore;

namespace user_api.Module;

public static class AddModules
{
    public static WebApplicationBuilder AddModuleService(this WebApplicationBuilder builder)
    {
        // ==========================================
        // 核心配置：接入远程 SSO 认证中心
        // ==========================================
        builder.Services.AddOpenIddict()
            .AddValidation(options =>
            {
                // 1. 设置认证中心的地址 (指向你刚才跑通的那个 SSO 项目的 URL)
                // 确保这个地址和 SSO 项目启动的地址完全一致
                options.SetIssuer("http://localhost:5010");

                // 2. 注册 System.Net.Http 扩展
                // API 会在启动时，自动向 http://localhost:5010/.well-known/openid-configuration 发起请求，下载验证 Token 所需的公钥
                options.UseSystemNetHttp();

                // 3. 注册 ASP.NET Core 主机
                options.UseAspNetCore();

            });
        // 告诉 ASP.NET Core，所有的身份验证都交给 OpenIddict Validation 来处理
        builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        builder.Services.AddAuthorization();


        return builder;

    }
}
