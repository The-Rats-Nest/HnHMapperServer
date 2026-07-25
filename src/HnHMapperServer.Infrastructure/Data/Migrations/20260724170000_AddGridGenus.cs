using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HnHMapperServer.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGridGenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Genus column to Grids table
            migrationBuilder.AddColumn<string>(
                name: "Genus",
                table: "Grids",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Add Genus column to CustomMarkers table
            migrationBuilder.AddColumn<string>(
                name: "Genus",
                table: "CustomMarkers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Recreate Grids table with new composite PK (Id, TenantId, Genus)
            // SQLite doesn't support ALTER TABLE to change primary keys, so we must recreate
            migrationBuilder.Sql(@"
                CREATE TABLE ""Grids_new"" (
                    ""Id"" TEXT NOT NULL,
                    ""TenantId"" TEXT NOT NULL,
                    ""Genus"" TEXT NOT NULL DEFAULT '',
                    ""CoordX"" INTEGER NOT NULL,
                    ""CoordY"" INTEGER NOT NULL,
                    ""Map"" INTEGER NOT NULL,
                    ""NextUpdate"" TEXT NOT NULL,
                    CONSTRAINT ""PK_Grids"" PRIMARY KEY (""Id"", ""TenantId"", ""Genus""),
                    CONSTRAINT ""FK_Grids_Tenants_TenantId"" FOREIGN KEY (""TenantId"") REFERENCES ""Tenants"" (""Id"") ON DELETE CASCADE
                );
                INSERT INTO ""Grids_new"" (""Id"", ""TenantId"", ""Genus"", ""CoordX"", ""CoordY"", ""Map"", ""NextUpdate"")
                    SELECT ""Id"", ""TenantId"", ""Genus"", ""CoordX"", ""CoordY"", ""Map"", ""NextUpdate"" FROM ""Grids"";
                DROP TABLE ""Grids"";
                ALTER TABLE ""Grids_new"" RENAME TO ""Grids"";
                CREATE INDEX ""IX_Grids_TenantId"" ON ""Grids"" (""TenantId"");
                CREATE INDEX ""IX_Grids_Map_CoordX_CoordY"" ON ""Grids"" (""Map"", ""CoordX"", ""CoordY"");
            ");

            // Update CustomMarkers FK index to include Genus
            migrationBuilder.DropIndex(
                name: "IX_CustomMarkers_GridId_TenantId",
                table: "CustomMarkers");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMarkers_GridId_TenantId_Genus",
                table: "CustomMarkers",
                columns: new[] { "GridId", "TenantId", "Genus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert CustomMarkers index
            migrationBuilder.DropIndex(
                name: "IX_CustomMarkers_GridId_TenantId_Genus",
                table: "CustomMarkers");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMarkers_GridId_TenantId",
                table: "CustomMarkers",
                columns: new[] { "GridId", "TenantId" });

            // Recreate Grids table with old composite PK (Id, TenantId)
            migrationBuilder.Sql(@"
                CREATE TABLE ""Grids_new"" (
                    ""Id"" TEXT NOT NULL,
                    ""TenantId"" TEXT NOT NULL,
                    ""CoordX"" INTEGER NOT NULL,
                    ""CoordY"" INTEGER NOT NULL,
                    ""Map"" INTEGER NOT NULL,
                    ""NextUpdate"" TEXT NOT NULL,
                    CONSTRAINT ""PK_Grids"" PRIMARY KEY (""Id"", ""TenantId""),
                    CONSTRAINT ""FK_Grids_Tenants_TenantId"" FOREIGN KEY (""TenantId"") REFERENCES ""Tenants"" (""Id"") ON DELETE CASCADE
                );
                INSERT INTO ""Grids_new"" (""Id"", ""TenantId"", ""CoordX"", ""CoordY"", ""Map"", ""NextUpdate"")
                    SELECT ""Id"", ""TenantId"", ""CoordX"", ""CoordY"", ""Map"", ""NextUpdate"" FROM ""Grids"";
                DROP TABLE ""Grids"";
                ALTER TABLE ""Grids_new"" RENAME TO ""Grids"";
                CREATE INDEX ""IX_Grids_TenantId"" ON ""Grids"" (""TenantId"");
                CREATE INDEX ""IX_Grids_Map_CoordX_CoordY"" ON ""Grids"" (""Map"", ""CoordX"", ""CoordY"");
            ");

            migrationBuilder.DropColumn(
                name: "Genus",
                table: "CustomMarkers");

            migrationBuilder.DropColumn(
                name: "Genus",
                table: "Grids");
        }
    }
}
