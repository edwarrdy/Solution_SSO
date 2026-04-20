using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 必须加这两个中间件，且顺序不能错！
app.UseAuthentication(); // 先查验 Token 身份
app.UseAuthorization();  // 后判断是否有权限

app.MapControllers();

app.Run();
