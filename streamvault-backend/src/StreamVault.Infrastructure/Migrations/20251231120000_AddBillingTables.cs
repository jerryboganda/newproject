using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StreamVault.Infrastructure.Data;

#nullable disable

namespace StreamVault.Infrastructure.Migrations
{
    [DbContext(typeof(StreamVaultDbContext))]
    [Migration("20251231120000_AddBillingTables")]
    public partial class AddBillingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsageMultipliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MetricType = table.Column<int>(type: "integer", nullable: false),
                    Multiplier = table.Column<double>(type: "double precision", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageMultipliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantBillingAccounts",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StripeCustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StripeDefaultPaymentMethodId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantBillingAccounts", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_TenantBillingAccounts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUsageSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StorageBytes = table.Column<long>(type: "bigint", nullable: false),
                    BandwidthBytes = table.Column<long>(type: "bigint", nullable: false),
                    VideoCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUsageSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantUsageSnapshots_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUsageMultiplierOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricType = table.Column<int>(type: "integer", nullable: false),
                    Multiplier = table.Column<double>(type: "double precision", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUsageMultiplierOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantUsageMultiplierOverrides_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManualPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManualPayments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantBillingPeriodInvoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodEndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StripeInvoiceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantBillingPeriodInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantBillingPeriodInvoices_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsageSnapshots_TenantId_PeriodStartUtc",
                table: "TenantUsageSnapshots",
                columns: new[] { "TenantId", "PeriodStartUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsageMultiplierOverrides_TenantId_MetricType",
                table: "TenantUsageMultiplierOverrides",
                columns: new[] { "TenantId", "MetricType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManualPayments_TenantId",
                table: "ManualPayments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantBillingPeriodInvoices_TenantId_PeriodStartUtc_PeriodEndUtc",
                table: "TenantBillingPeriodInvoices",
                columns: new[] { "TenantId", "PeriodStartUtc", "PeriodEndUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsageSnapshots_TenantId",
                table: "TenantUsageSnapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsageMultiplierOverrides_TenantId",
                table: "TenantUsageMultiplierOverrides",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantBillingPeriodInvoices_TenantId",
                table: "TenantBillingPeriodInvoices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageMultipliers_MetricType_IsActive",
                table: "UsageMultipliers",
                columns: new[] { "MetricType", "IsActive" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TenantBillingPeriodInvoices");
            migrationBuilder.DropTable(name: "ManualPayments");
            migrationBuilder.DropTable(name: "TenantUsageMultiplierOverrides");
            migrationBuilder.DropTable(name: "TenantUsageSnapshots");
            migrationBuilder.DropTable(name: "TenantBillingAccounts");
            migrationBuilder.DropTable(name: "UsageMultipliers");
        }
    }
}
