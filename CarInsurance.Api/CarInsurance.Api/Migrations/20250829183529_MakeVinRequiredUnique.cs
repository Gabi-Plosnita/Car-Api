using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarInsurance.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakeVinRequiredUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cars_Vin",
                table: "Cars");

            migrationBuilder.AlterColumn<string>(
                name: "Vin",
                table: "Cars",
                type: "TEXT",
                maxLength: 17,
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Vin",
                table: "Cars",
                column: "Vin",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cars_Vin",
                table: "Cars");

            migrationBuilder.AlterColumn<string>(
                name: "Vin",
                table: "Cars",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 17,
                oldCollation: "NOCASE");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Vin",
                table: "Cars",
                column: "Vin");
        }
    }
}
