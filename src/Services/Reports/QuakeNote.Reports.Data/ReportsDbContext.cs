using Microsoft.EntityFrameworkCore;
using QuakeNote.Reports.Data.Entities;

namespace QuakeNote.Reports.Data;

public class ReportsDbContext(DbContextOptions<ReportsDbContext> options) : DbContext(options)
{
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<DetailedReport> DetailedReports => Set<DetailedReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportsDbContext).Assembly);
}