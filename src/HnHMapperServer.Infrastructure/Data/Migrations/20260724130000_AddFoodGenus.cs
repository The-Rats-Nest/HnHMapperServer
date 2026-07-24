using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HnHMapperServer.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodGenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genus",
                table: "Foods",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropIndex(
                name: "IX_Foods_TenantId_Name",
                table: "Foods");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_TenantId_Name_Genus",
                table: "Foods",
                columns: new[] { "TenantId", "Name", "Genus" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Foods_TenantId_Name_Genus",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "Genus",
                table: "Foods");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_TenantId_Name",
                table: "Foods",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }
    }
}
