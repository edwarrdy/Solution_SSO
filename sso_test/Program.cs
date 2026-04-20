using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SqlSugar;
using sso_test;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Server=127.0.0.1;Port=3306;Database=sso;Uid=root;Pwd=123456;";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 1. 注册 OpenIddict 专属的 EF Core DbContext (使用 MySQL)
builder.Services.AddDbContext<OidcDbContext>(options =>
{
    // 配置使用 Pomelo 的 MySQL 驱动
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

    // 注册 OpenIddict 的实体
    options.UseOpenIddict();
});

// 2. 注册 OpenIddict
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        // 告诉 OpenIddict：你的数据存在这个 DbContext 里
        options.UseEntityFrameworkCore()
               .UseDbContext<OidcDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token");
        options.AllowPasswordFlow()
               .AllowRefreshTokenFlow(); // 有了数据库，终于可以开启刷新令牌了！

        options.AcceptAnonymousClients();
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();
        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .DisableTransportSecurityRequirement();

        options.DisableAccessTokenEncryption();
    });

builder.Services.AddScoped<ISqlSugarClient>(s =>
{
    // 这里可以使用 SqlSugarClient
    var sqlSugar = new SqlSugarClient(new ConnectionConfig()
    {
        ConnectionString = connectionString,
        DbType = DbType.MySql,
        IsAutoCloseConnection = true
    });

    return sqlSugar;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OidcDbContext>();
    // 如果表不存在，自动创建（适合开发阶段快速起步）
    context.Database.EnsureCreated();
}

app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
