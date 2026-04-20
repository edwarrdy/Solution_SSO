using Microsoft.EntityFrameworkCore;

namespace sso_test;

public class OidcDbContext : DbContext
{
    public OidcDbContext(DbContextOptions<OidcDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 这一行非常关键：它会自动帮你把那 4 张表的结构映射好
        builder.UseOpenIddict();
    }
}
