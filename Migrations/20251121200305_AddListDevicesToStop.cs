using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddListDevicesToStop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stops_Devices_DeviceId",
                table: "Stops");

            migrationBuilder.DropIndex(
                name: "IX_Stops_DeviceId",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Stops");

            migrationBuilder.CreateTable(
                name: "DeviceStop",
                columns: table => new
                {
                    DevicesId = table.Column<int>(type: "int", nullable: false),
                    StopsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStop", x => new { x.DevicesId, x.StopsId });
                    table.ForeignKey(
                        name: "FK_DeviceStop_Devices_DevicesId",
                        column: x => x.DevicesId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceStop_Stops_StopsId",
                        column: x => x.StopsId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStop_StopsId",
                table: "DeviceStop",
                column: "StopsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceStop");

            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "Stops",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stops_DeviceId",
                table: "Stops",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stops_Devices_DeviceId",
                table: "Stops",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");
        }
    }
}
