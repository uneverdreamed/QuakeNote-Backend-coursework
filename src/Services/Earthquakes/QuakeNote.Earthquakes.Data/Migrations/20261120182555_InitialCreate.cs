using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuakeNote.Earthquakes.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "earthquakes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    external_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    epicenter_lat = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    epicenter_lon = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    depth = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    magnitude = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    region_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_earthquakes", x => x.id);
                    table.CheckConstraint("ck_earthquakes_depth", "depth >= 0");
                    table.CheckConstraint("ck_earthquakes_epicenter_lat", "epicenter_lat between -90 and 90");
                    table.CheckConstraint("ck_earthquakes_epicenter_lon", "epicenter_lon between -180 and 180");
                    table.CheckConstraint("ck_earthquakes_magnitude", "magnitude between 0 and 10");
                });

            migrationBuilder.CreateTable(
                name: "seismic_stations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    organization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_seismic_stations", x => x.id);
                    table.CheckConstraint("ck_seismic_stations_latitude", "latitude between -90 and 90");
                    table.CheckConstraint("ck_seismic_stations_longitude", "longitude between -180 and 180");
                });

            migrationBuilder.CreateTable(
                name: "event_assessment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    earthquake_id = table.Column<int>(type: "integer", nullable: false),
                    instrumental_intensity = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    macroseismic_intensity = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    verification_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "insufficient_data"),
                    reports_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_recomputed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_assessment", x => x.id);
                    table.CheckConstraint("ck_event_assessment_reports_count", "reports_count >= 0");
                    table.CheckConstraint("ck_event_assessment_status", "verification_status in ('confirmed','disputed','insufficient_data')");
                    table.ForeignKey(
                        name: "fk_event_assessment_earthquakes_earthquake_id",
                        column: x => x.earthquake_id,
                        principalTable: "earthquakes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "station_readings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    earthquake_id = table.Column<int>(type: "integer", nullable: false),
                    station_id = table.Column<int>(type: "integer", nullable: false),
                    pga = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    pgv = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: false),
                    pgd = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: true),
                    arias_intensity = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: true),
                    p_wave_arrival_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    distance_to_epicenter_km = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_station_readings", x => x.id);
                    table.CheckConstraint("ck_station_readings_distance", "distance_to_epicenter_km >= 0");
                    table.CheckConstraint("ck_station_readings_pga", "pga >= 0");
                    table.CheckConstraint("ck_station_readings_pgv", "pgv >= 0");
                    table.ForeignKey(
                        name: "fk_station_readings_earthquakes_earthquake_id",
                        column: x => x.earthquake_id,
                        principalTable: "earthquakes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_station_readings_seismic_stations_station_id",
                        column: x => x.station_id,
                        principalTable: "seismic_stations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_earthquakes_epicenter_lat_epicenter_lon",
                table: "earthquakes",
                columns: new[] { "epicenter_lat", "epicenter_lon" });

            migrationBuilder.CreateIndex(
                name: "ix_earthquakes_external_id",
                table: "earthquakes",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_earthquakes_magnitude",
                table: "earthquakes",
                column: "magnitude");

            migrationBuilder.CreateIndex(
                name: "ix_earthquakes_occurred_at",
                table: "earthquakes",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_earthquakes_uid",
                table: "earthquakes",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_assessment_earthquake_id",
                table: "event_assessment",
                column: "earthquake_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_seismic_stations_code",
                table: "seismic_stations",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_seismic_stations_latitude_longitude",
                table: "seismic_stations",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "ix_seismic_stations_uid",
                table: "seismic_stations",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_station_readings_earthquake_id_station_id",
                table: "station_readings",
                columns: new[] { "earthquake_id", "station_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_station_readings_station_id",
                table: "station_readings",
                column: "station_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_assessment");

            migrationBuilder.DropTable(
                name: "station_readings");

            migrationBuilder.DropTable(
                name: "earthquakes");

            migrationBuilder.DropTable(
                name: "seismic_stations");
        }
    }
}
