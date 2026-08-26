using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFN.Sign.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sign");

            migrationBuilder.CreateTable(
                name: "MessageTemplates",
                schema: "sign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    TemplateType = table.Column<int>(type: "integer", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BodyTemplate = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SignRequests",
                schema: "sign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentSignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Recipient = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SignedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    VerifyAttemptsUsed = table.Column<int>(type: "integer", nullable: false),
                    SendAttemptsUsed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SignAttempts",
                schema: "sign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SignRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignAttempts_SignRequests_SignRequestId",
                        column: x => x.SignRequestId,
                        principalSchema: "sign",
                        principalTable: "SignRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignCodes",
                schema: "sign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SignRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CodeSalt = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignCodes_SignRequests_SignRequestId",
                        column: x => x.SignRequestId,
                        principalSchema: "sign",
                        principalTable: "SignRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplates_Channel_IsActive",
                schema: "sign",
                table: "MessageTemplates",
                columns: new[] { "Channel", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplates_TemplateType",
                schema: "sign",
                table: "MessageTemplates",
                column: "TemplateType");

            migrationBuilder.CreateIndex(
                name: "IX_SignAttempts_SignRequestId",
                schema: "sign",
                table: "SignAttempts",
                column: "SignRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SignCodes_SignRequestId",
                schema: "sign",
                table: "SignCodes",
                column: "SignRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignCodes_SignRequestId_IsUsed",
                schema: "sign",
                table: "SignCodes",
                columns: new[] { "SignRequestId", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_SignRequests_DocumentSignId",
                schema: "sign",
                table: "SignRequests",
                column: "DocumentSignId");

            migrationBuilder.CreateIndex(
                name: "IX_SignRequests_Status",
                schema: "sign",
                table: "SignRequests",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageTemplates",
                schema: "sign");

            migrationBuilder.DropTable(
                name: "SignAttempts",
                schema: "sign");

            migrationBuilder.DropTable(
                name: "SignCodes",
                schema: "sign");

            migrationBuilder.DropTable(
                name: "SignRequests",
                schema: "sign");
        }
    }
}
