using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuakeNote.Reports.Data;

public class ReportsDbContextFactory : IDesignTimeDbContextFactory<ReportsDbContext>
{
    public ReportsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time_only;Username=design;Password=design")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new ReportsDbContext(options);
    }
}