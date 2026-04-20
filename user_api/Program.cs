using user_api.Module;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddModuleService();

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
