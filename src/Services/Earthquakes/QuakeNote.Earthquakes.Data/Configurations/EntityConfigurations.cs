using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuakeNote.Earthquakes.Data.Entities;

namespace QuakeNote.Earthquakes.Data.Configurations;

public class EarthquakeConfiguration : IEntityTypeConfiguration<Earthquake>
{
    public void Configure(EntityTypeBuilder<Earthquake> b)
    {
        b.ToTable("earthquakes", t =>
        {
            t.HasCheckConstraint("ck_earthquakes_epicenter_lat", "epicenter_lat between -90 and 90");
            t.HasCheckConstraint("ck_earthquakes_epicenter_lon", "epicenter_lon between -180 and 180");
            t.HasCheckConstraint("ck_earthquakes_magnitude", "magnitude between 0 and 10");
            t.HasCheckConstraint("ck_earthquakes_depth", "depth >= 0");
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.Uid).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.ExternalId).HasMaxLength(50).IsRequired();
        b.Property(x => x.EpicenterLat).HasPrecision(9, 6);
        b.Property(x => x.EpicenterLon).HasPrecision(9, 6);
        b.Property(x => x.Depth).HasPrecision(6, 2);
        b.Property(x => x.Magnitude).HasPrecision(3, 1);
        b.Property(x => x.RegionName).HasMaxLength(50).IsRequired();
        b.Property(x => x.Source).HasMaxLength(100).IsRequired();

        b.HasIndex(x => x.Uid).IsUnique();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.HasIndex(x => x.OccurredAt);
        b.HasIndex(x => x.Magnitude);
        b.HasIndex(x => new { x.EpicenterLat, x.EpicenterLon });

        b.HasOne(x => x.Assessment)
            .WithOne(a => a.Earthquake)
            .HasForeignKey<EventAssessment>(a => a.EarthquakeId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Readings)
            .WithOne(r => r.Earthquake)
            .HasForeignKey(r => r.EarthquakeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SeismicStationConfiguration : IEntityTypeConfiguration<SeismicStation>
{
    public void Configure(EntityTypeBuilder<SeismicStation> b)
    {
        b.ToTable("seismic_stations", t =>
        {
            t.HasCheckConstraint("ck_seismic_stations_latitude", "latitude between -90 and 90");
            t.HasCheckConstraint("ck_seismic_stations_longitude", "longitude between -180 and 180");
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.Uid).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Latitude).HasPrecision(9, 6);
        b.Property(x => x.Longitude).HasPrecision(9, 6);
        b.Property(x => x.Region).HasMaxLength(50).IsRequired();
        b.Property(x => x.Organization).HasMaxLength(100).IsRequired();

        b.HasIndex(x => x.Uid).IsUnique();
        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => new { x.Latitude, x.Longitude });
    }
}

public class StationReadingConfiguration : IEntityTypeConfiguration<StationReading>
{
    public void Configure(EntityTypeBuilder<StationReading> b)
    {
        b.ToTable("station_readings", t =>
        {
            t.HasCheckConstraint("ck_station_readings_pga", "pga >= 0");
            t.HasCheckConstraint("ck_station_readings_pgv", "pgv >= 0");
            t.HasCheckConstraint("ck_station_readings_distance", "distance_to_epicenter_km >= 0");
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.Pga).HasPrecision(8, 3);
        b.Property(x => x.Pgv).HasPrecision(8, 3);
        b.Property(x => x.Pgd).HasPrecision(8, 3);
        b.Property(x => x.AriasIntensity).HasPrecision(8, 4);
        b.Property(x => x.DistanceToEpicenterKm).HasPrecision(8, 2);

        // одна запись на пару «событие — станция»
        b.HasIndex(x => new { x.EarthquakeId, x.StationId }).IsUnique();

        // станцию с показаниями удалить нельзя
        b.HasOne(x => x.Station)
            .WithMany(s => s.Readings)
            .HasForeignKey(x => x.StationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EventAssessmentConfiguration : IEntityTypeConfiguration<EventAssessment>
{
    public void Configure(EntityTypeBuilder<EventAssessment> b)
    {
        b.ToTable("event_assessment", t =>
        {
            t.HasCheckConstraint("ck_event_assessment_status",
                "verification_status in ('confirmed','disputed','insufficient_data')");
            t.HasCheckConstraint("ck_event_assessment_reports_count", "reports_count >= 0");
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.InstrumentalIntensity).HasPrecision(3, 1);
        b.Property(x => x.MacroseismicIntensity).HasPrecision(3, 1);
        b.Property(x => x.VerificationStatus).HasMaxLength(20).HasDefaultValue("insufficient_data");
        b.Property(x => x.ReportsCount).HasDefaultValue(0);
        b.Property(x => x.LastRecomputedAt).HasDefaultValueSql("now()");

        b.HasIndex(x => x.EarthquakeId).IsUnique();
    }
}