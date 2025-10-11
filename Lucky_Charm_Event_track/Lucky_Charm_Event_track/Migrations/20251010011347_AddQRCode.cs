using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyCharmEventtrack.Migrations
{
    /// <inheritdoc />
    public partial class AddQRCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CheckedIn",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QRCodeText",
                table: "Tickets",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckedIn",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "QRCodeText",
                table: "Tickets");
        }
    }
}
