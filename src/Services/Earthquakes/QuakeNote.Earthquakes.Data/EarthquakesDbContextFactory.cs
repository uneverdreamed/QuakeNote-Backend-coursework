using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuakeNote.Earthquakes.Data;

public class EarthquakesDbContextFactory : IDesignTimeDbContextFactory<EarthquakesDbContext>
{
    public EarthquakesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EarthquakesDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time_only;Username=design;Password=design")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new EarthquakesDbContext(options);
    }
}