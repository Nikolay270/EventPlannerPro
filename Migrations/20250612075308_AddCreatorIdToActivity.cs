using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPlannerPro.Migrations
{
    public partial class AddCreatorIdToActivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Activities",
                newName: "CreatorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Activities",
                newName: "CreatedById");
        }
    }
}
