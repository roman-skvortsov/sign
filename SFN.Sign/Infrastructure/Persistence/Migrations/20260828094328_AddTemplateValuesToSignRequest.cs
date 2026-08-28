using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFN.Sign.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateValuesToSignRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateValues",
                schema: "sign",
                table: "SignRequests",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateValues",
                schema: "sign",
                table: "SignRequests");
        }
    }
}
