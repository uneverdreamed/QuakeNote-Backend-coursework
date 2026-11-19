using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuakeNote.Reports.Data.Entities;

namespace QuakeNote.Reports.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> b)
    {
        b.ToTable("reports", t =>
        {
            t.HasCheckConstraint("ck_reports_felt_intensity", "felt_intensity between 1 and 10");
            t.HasCheckConstraint("ck_reports_latitude", "latitude between -90 and 90");
            t.HasCheckConstraint("ck_reports_longitude", "longitude between -180 and 180");
            t.HasCheckConstraint("ck_reports_validation_status",
                "validation_status in ('pending','verified','outlier','unverified')");
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.Uid).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Latitude).HasPrecision(9, 6);
        b.Property(x => x.Longitude).HasPrecision(9, 6);
        b.Property(x => x.RegionName).HasMaxLength(50);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.ValidationStatus).HasMaxLength(20).HasDefaultValue("pending");
        b.Property(x => x.ExpectedIntensity).HasPrecision(3, 1);
        b.Property(x => x.Deviation).HasPrecision(3, 1);

        b.HasIndex(x => x.Uid).IsUnique();
        b.HasIndex(x => x.UserUid);
        b.HasIndex(x => x.EarthquakeUid);
        b.HasIndex(x => x.CreatedAt);

        b.HasOne(x => x.Detail)
            .WithOne(d => d.Report)
            .HasForeignKey<DetailedReport>(d => d.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DetailedReportConfiguration : IEntityTypeConfiguration<DetailedReport>
{
    public void Configure(EntityTypeBuilder<DetailedReport> b)
    {
        b.ToTable("detailed_reports");
        b.HasKey(x => x.Id);

        b.Property(x => x.Duration).HasMaxLength(20);
        b.Property(x => x.LocationType).HasMaxLength(50);
        b.Property(x => x.HumanAction).HasMaxLength(100);
        b.Property(x => x.OscillationType).HasMaxLength(50);
        b.Property(x => x.WitnessesCount).HasMaxLength(50);
        b.Property(x => x.CrackWidth).HasMaxLength(20);
        b.Property(x => x.CrackLength).HasMaxLength(20);
        b.Property(x => x.ContactPhone).HasMaxLength(20);
        b.Property(x => x.ContactEmail).HasMaxLength(255);
        b.Property(x => x.ContactName).HasMaxLength(200);

        b.Property(x => x.Reactions).HasColumnType("text[]");
        b.Property(x => x.AffectedObjects).HasColumnType("text[]");
        b.Property(x => x.BuildingElements).HasColumnType("text[]");
        b.Property(x => x.ObservationsInside).HasColumnType("text[]");
        b.Property(x => x.OutdoorObjects).HasColumnType("text[]");
        b.Property(x => x.Infrastructure).HasColumnType("text[]");
        b.Property(x => x.SurfacePhenomena).HasColumnType("text[]");
        b.Property(x => x.OtherObservations).HasColumnType("text[]");

        b.HasIndex(x => x.ReportId).IsUnique();
    }
}