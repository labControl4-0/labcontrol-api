using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabControlApi.Migrations
{
    public partial class AddPlantGeolocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Plants",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Plants",
                type: "double precision",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Latitude", table: "Plants");
            migrationBuilder.DropColumn(name: "Longitude", table: "Plants");
        }
    }
}
