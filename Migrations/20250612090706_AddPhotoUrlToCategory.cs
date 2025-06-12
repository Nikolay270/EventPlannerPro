using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPlannerPro.Migrations
{
    public partial class AddPhotoUrlToCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove default categories if needed
            migrationBuilder.Sql("DELETE FROM Categories");

            // Drop any duplicate roles to avoid key violation (optional safety)
            migrationBuilder.Sql("DELETE FROM AspNetRoles WHERE NormalizedName IN ('ADMIN', 'USER')");

            // Add the new PhotoUrl column
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the PhotoUrl column on rollback
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Categories");
        }
    }
}
