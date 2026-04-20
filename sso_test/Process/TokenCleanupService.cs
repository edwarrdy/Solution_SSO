using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace sso_test.Process;

public class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public TokenCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // 注意：这里直接获取你的 OidcDbContext
                    var context = scope.ServiceProvider.GetRequiredService<OidcDbContext>();

                    // 获取当前 UTC 时间
                    var now = DateTime.UtcNow;

                    // 1. 物理删除所有已过期的令牌 (Access Token, Refresh Token 等)
                    // 这种简单的 WHERE 语句在 MySQL 中是性能最强且绝不会报错的
                    int deletedTokens = await context.Database.ExecuteSqlRawAsync(
                        "DELETE FROM OpenIddictTokens WHERE ExpirationDate < {0}",
                        new object[] { now },
                        stoppingToken);

                    // 2. 物理删除所有非有效状态的授权记录
                    int deletedAuths = await context.Database.ExecuteSqlRawAsync(
                        "DELETE FROM OpenIddictAuthorizations WHERE Status <> 'valid'",
                        stoppingToken);

                    if (deletedTokens > 0 || deletedAuths > 0)
                    {
                        Console.WriteLine($"[清理任务] 已清理 {deletedTokens} 个过期令牌和 {deletedAuths} 条无效授权。");
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录异常，但不中断循环
                Console.WriteLine($"[清理任务失败] 原因: {ex.Message}");
            }

            // 每天执行一次
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
