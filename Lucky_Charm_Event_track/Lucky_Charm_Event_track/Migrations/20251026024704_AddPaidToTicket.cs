using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyCharmEventtrack.Migrations
{
    /// <inheritdoc />
    public partial class AddPaidToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Paid",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Paid",
                table: "Tickets");
        }
    }
}
