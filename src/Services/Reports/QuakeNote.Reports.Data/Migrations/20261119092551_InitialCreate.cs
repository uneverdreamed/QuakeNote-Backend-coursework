using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuakeNote.Reports.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    earthquake_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    felt_intensity = table.Column<short>(type: "smallint", nullable: false),
                    region_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    matched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    validation_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    expected_intensity = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    deviation = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reports", x => x.id);
                    table.CheckConstraint("ck_reports_felt_intensity", "felt_intensity between 1 and 10");
                    table.CheckConstraint("ck_reports_latitude", "latitude between -90 and 90");
                    table.CheckConstraint("ck_reports_longitude", "longitude between -180 and 180");
                    table.CheckConstraint("ck_reports_validation_status", "validation_status in ('pending','verified','outlier','unverified')");
                });

            migrationBuilder.CreateTable(
                name: "detailed_reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_id = table.Column<int>(type: "integer", nullable: false),
                    event_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    location_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    human_action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    oscillation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reactions = table.Column<List<string>>(type: "text[]", nullable: false),
                    witnesses_count = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    affected_objects = table.Column<List<string>>(type: "text[]", nullable: false),
                    building_elements = table.Column<List<string>>(type: "text[]", nullable: false),
                    observations_inside = table.Column<List<string>>(type: "text[]", nullable: false),
                    outdoor_objects = table.Column<List<string>>(type: "text[]", nullable: false),
                    infrastructure = table.Column<List<string>>(type: "text[]", nullable: false),
                    surface_phenomena = table.Column<List<string>>(type: "text[]", nullable: false),
                    crack_width = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    crack_length = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    other_observations = table.Column<List<string>>(type: "text[]", nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    additional_info = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_detailed_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_detailed_reports_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_detailed_reports_report_id",
                table: "detailed_reports",
                column: "report_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reports_created_at",
                table: "reports",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_reports_earthquake_uid",
                table: "reports",
                column: "earthquake_uid");

            migrationBuilder.CreateIndex(
                name: "ix_reports_uid",
                table: "reports",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reports_user_uid",
                table: "reports",
                column: "user_uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detailed_reports");

            migrationBuilder.DropTable(
                name: "reports");
        }
    }
}
