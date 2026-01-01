using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StreamVault.Infrastructure.Data;

#nullable disable

namespace StreamVault.Infrastructure.Migrations
{
    [DbContext(typeof(StreamVaultDbContext))]
    [Migration("20251231130000_AddAnalyticsTables")]
    public partial class AddAnalyticsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    PositionSeconds = table.Column<double>(type: "double precision", nullable: true),
                    DeviceType = table.Column<string>(type: "text", nullable: true),
                    Browser = table.Column<string>(type: "text", nullable: true),
                    OS = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    Referrer = table.Column<string>(type: "text", nullable: true),
                    UTMSource = table.Column<string>(type: "text", nullable: true),
                    UTMMedium = table.Column<string>(type: "text", nullable: true),
                    UTMCampaign = table.Column<string>(type: "text", nullable: true),
                    SessionId = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoAnalytics_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoAnalytics_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoAnalytics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoAnalyticsHourlyAggregates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    BucketStartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Views = table.Column<int>(type: "integer", nullable: false),
                    UniqueViewers = table.Column<int>(type: "integer", nullable: false),
                    WatchTimeSeconds = table.Column<double>(type: "double precision", nullable: false),
                    Completes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAnalyticsHourlyAggregates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoAnalyticsHourlyAggregates_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoAnalyticsHourlyAggregates_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoAnalyticsDailyAggregates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    Views = table.Column<int>(type: "integer", nullable: false),
                    UniqueViewers = table.Column<int>(type: "integer", nullable: false),
                    WatchTimeSeconds = table.Column<double>(type: "double precision", nullable: false),
                    Completes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAnalyticsDailyAggregates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoAnalyticsDailyAggregates_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoAnalyticsDailyAggregates_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoAnalyticsDailyCountryAggregates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Views = table.Column<int>(type: "integer", nullable: false),
                    UniqueViewers = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAnalyticsDailyCountryAggregates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoAnalyticsDailyCountryAggregates_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoAnalyticsDailyCountryAggregates_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalytics_TenantId_Timestamp",
                table: "VideoAnalytics",
                columns: new[] { "TenantId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalytics_TenantId_VideoId_Timestamp",
                table: "VideoAnalytics",
                columns: new[] { "TenantId", "VideoId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalytics_VideoId_Timestamp",
                table: "VideoAnalytics",
                columns: new[] { "VideoId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalytics_UserId",
                table: "VideoAnalytics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalyticsHourlyAggregates_TenantId_VideoId_BucketStartUtc",
                table: "VideoAnalyticsHourlyAggregates",
                columns: new[] { "TenantId", "VideoId", "BucketStartUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalyticsHourlyAggregates_VideoId",
                table: "VideoAnalyticsHourlyAggregates",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalyticsDailyAggregates_TenantId_VideoId_DateUtc",
                table: "VideoAnalyticsDailyAggregates",
                columns: new[] { "TenantId", "VideoId", "DateUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalyticsDailyAggregates_VideoId",
                table: "VideoAnalyticsDailyAggregates",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalyticsDailyCountryAggregates_TenantId_VideoId_DateUtc_CountryCode",
                table: "VideoAnalyticsDailyCountryAggregates",
                columns: new[] { "TenantId", "VideoId", "DateUtc", "CountryCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoAnalyticsDailyCountryAggregates_VideoId",
                table: "VideoAnalyticsDailyCountryAggregates",
                column: "VideoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "VideoAnalyticsDailyCountryAggregates");
            migrationBuilder.DropTable(name: "VideoAnalyticsDailyAggregates");
            migrationBuilder.DropTable(name: "VideoAnalyticsHourlyAggregates");
            migrationBuilder.DropTable(name: "VideoAnalytics");
        }
    }
}
