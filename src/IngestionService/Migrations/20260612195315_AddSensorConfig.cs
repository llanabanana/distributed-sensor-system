using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngestionService.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorConfigs",
                columns: table => new
                {
                    SensorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinTemperature = table.Column<double>(type: "double precision", nullable: false),
                    MaxTemperature = table.Column<double>(type: "double precision", nullable: false),
                    Quality = table.Column<int>(type: "integer", nullable: false),
                    AlarmLowThreshold = table.Column<double>(type: "double precision", nullable: false),
                    AlarmMedThreshold = table.Column<double>(type: "double precision", nullable: false),
                    AlarmHighThreshold = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorConfigs", x => x.SensorId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorConfigs");
        }
    }
}
