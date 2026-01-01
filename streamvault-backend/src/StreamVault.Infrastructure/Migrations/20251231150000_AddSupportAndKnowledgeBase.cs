using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StreamVault.Infrastructure.Data;

#nullable disable

namespace StreamVault.Infrastructure.Migrations
{
    [DbContext(typeof(StreamVaultDbContext))]
    [Migration("20251231150000_AddSupportAndKnowledgeBase")]
    public partial class AddSupportAndKnowledgeBase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportSlaPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FirstResponseMinutes = table.Column<int>(type: "integer", nullable: false),
                    ResolutionMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportSlaPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportSlaPolicies_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportDepartments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultSlaPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportDepartments_SupportSlaPolicies_DefaultSlaPolicyId",
                        column: x => x.DefaultSlaPolicyId,
                        principalTable: "SupportSlaPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupportDepartments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportEscalationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    ThresholdMinutes = table.Column<int>(type: "integer", nullable: false),
                    EscalateToPriority = table.Column<int>(type: "integer", nullable: false),
                    SetStatusToEscalated = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportEscalationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportEscalationRules_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeBaseCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeBaseCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeBaseCategories_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeBaseArticles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Views = table.Column<int>(type: "integer", nullable: false),
                    HelpfulVotes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeBaseArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeBaseArticles_KnowledgeBaseCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "KnowledgeBaseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KnowledgeBaseArticles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KnowledgeBaseArticles_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KnowledgeBaseArticles_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CannedResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Shortcuts = table.Column<string[]>(type: "text[]", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CannedResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CannedResponses_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CannedResponses_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uuid", nullable: true),
                    TicketNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlaPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Resolution = table.Column<string>(type: "text", nullable: true),
                    EscalationReason = table.Column<string>(type: "text", nullable: true),
                    FirstResponseAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FirstResponseDueAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ResolutionDueAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTickets_SupportDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "SupportDepartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTickets_SupportSlaPolicies_SlaPolicyId",
                        column: x => x.SlaPolicyId,
                        principalTable: "SupportSlaPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportTicketReplies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTicketReplies_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTicketReplies_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTicketReplies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportTicketActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTicketActivities_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTicketActivities_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTicketActivities_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportSlaPolicies_TenantId_Name",
                table: "SupportSlaPolicies",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportSlaPolicies_TenantId",
                table: "SupportSlaPolicies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportDepartments_TenantId_Slug",
                table: "SupportDepartments",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportDepartments_DefaultSlaPolicyId",
                table: "SupportDepartments",
                column: "DefaultSlaPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportDepartments_TenantId",
                table: "SupportDepartments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportEscalationRules_TenantId_IsActive",
                table: "SupportEscalationRules",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportEscalationRules_TenantId",
                table: "SupportEscalationRules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBaseCategories_TenantId_Slug",
                table: "KnowledgeBaseCategories",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBaseCategories_TenantId_SortOrder",
                table: "KnowledgeBaseCategories",
                columns: new[] { "TenantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBaseCategories_TenantId",
                table: "KnowledgeBaseCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBaseArticles_TenantId_Slug",
                table: "KnowledgeBaseArticles",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBaseArticles_TenantId_CategoryId_IsPublished",
                table: "KnowledgeBaseArticles",
                columns: new[] { "TenantId", "CategoryId", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBaseArticles_CategoryId",
                table: "KnowledgeBaseArticles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBaseArticles_TenantId",
                table: "KnowledgeBaseArticles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBaseArticles_CreatedByUserId",
                table: "KnowledgeBaseArticles",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBaseArticles_UpdatedByUserId",
                table: "KnowledgeBaseArticles",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CannedResponses_TenantId_IsActive",
                table: "CannedResponses",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CannedResponses_TenantId",
                table: "CannedResponses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CannedResponses_CreatedByUserId",
                table: "CannedResponses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TenantId_TicketNumber",
                table: "SupportTickets",
                columns: new[] { "TenantId", "TicketNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TenantId_Status",
                table: "SupportTickets",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TenantId_CreatedAt",
                table: "SupportTickets",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TenantId",
                table: "SupportTickets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_UserId",
                table: "SupportTickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AssignedToId",
                table: "SupportTickets",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_DepartmentId",
                table: "SupportTickets",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_SlaPolicyId",
                table: "SupportTickets",
                column: "SlaPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketReplies_TenantId_TicketId_CreatedAt",
                table: "SupportTicketReplies",
                columns: new[] { "TenantId", "TicketId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketReplies_TicketId",
                table: "SupportTicketReplies",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketReplies_TenantId",
                table: "SupportTicketReplies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketReplies_UserId",
                table: "SupportTicketReplies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketActivities_TenantId_TicketId_CreatedAt",
                table: "SupportTicketActivities",
                columns: new[] { "TenantId", "TicketId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketActivities_TicketId",
                table: "SupportTicketActivities",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketActivities_TenantId",
                table: "SupportTicketActivities",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketActivities_CreatedByUserId",
                table: "SupportTicketActivities",
                column: "CreatedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SupportTicketActivities");
            migrationBuilder.DropTable(name: "SupportTicketReplies");
            migrationBuilder.DropTable(name: "CannedResponses");
            migrationBuilder.DropTable(name: "KnowledgeBaseArticles");
            migrationBuilder.DropTable(name: "SupportTickets");
            migrationBuilder.DropTable(name: "KnowledgeBaseCategories");
            migrationBuilder.DropTable(name: "SupportEscalationRules");
            migrationBuilder.DropTable(name: "SupportDepartments");
            migrationBuilder.DropTable(name: "SupportSlaPolicies");
        }
    }
}
