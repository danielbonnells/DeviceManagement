using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceManagement.Migrations
{
    /// <inheritdoc />
    public partial class EditToIncludeNullableProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StopType",
                table: "Stops",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<float>(
                name: "StopLon",
                table: "Stops",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "StopLat",
                table: "Stops",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "ParentStation",
                table: "Stops",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "StopType",
                keyValue: null,
                column: "StopType",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "StopType",
                table: "Stops",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<float>(
                name: "StopLon",
                table: "Stops",
                type: "float",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "StopLat",
                table: "Stops",
                type: "float",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Stops",
                keyColumn: "ParentStation",
                keyValue: null,
                column: "ParentStation",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ParentStation",
                table: "Stops",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
