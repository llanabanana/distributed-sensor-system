using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngestionService.Migrations
{
    /// <inheritdoc />
    public partial class AddIsReserveToSensorStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReserve",
                table: "SensorStatuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReserve",
                table: "SensorStatuses");
        }
    }
}
