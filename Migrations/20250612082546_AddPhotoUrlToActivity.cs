using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPlannerPro.Migrations
{
    public partial class AddPhotoUrlToActivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Activities");
        }
    }
}
