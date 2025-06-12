using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPlannerPro.Migrations
{
    public partial class AddPhotoUrlToCity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.Sql("DELETE FROM Cities WHERE Id IN (1, 2, 3)");

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Cities");
        }
    }
}
