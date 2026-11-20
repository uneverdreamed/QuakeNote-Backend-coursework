using Microsoft.EntityFrameworkCore;
using QuakeNote.Earthquakes.Data.Entities;

namespace QuakeNote.Earthquakes.Data;

public class EarthquakesDbContext(DbContextOptions<EarthquakesDbContext> options) : DbContext(options)
{
    public DbSet<Earthquake> Earthquakes => Set<Earthquake>();
    public DbSet<SeismicStation> Stations => Set<SeismicStation>();
    public DbSet<StationReading> Readings => Set<StationReading>();
    public DbSet<EventAssessment> Assessments => Set<EventAssessment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(EarthquakesDbContext).Assembly);
}