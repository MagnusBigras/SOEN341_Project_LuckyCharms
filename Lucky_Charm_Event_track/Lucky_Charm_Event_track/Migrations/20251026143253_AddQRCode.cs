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
            migrationBuilder.AddColumn<byte[]>(
                name: "QRCode",
                table: "Tickets",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "Tickets");
        }
    }
}
