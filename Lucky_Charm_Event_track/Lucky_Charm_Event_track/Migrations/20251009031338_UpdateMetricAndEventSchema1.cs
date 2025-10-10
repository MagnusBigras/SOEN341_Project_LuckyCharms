using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyCharmEventtrack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMetricAndEventSchema1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Metrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TotalRevenue = table.Column<double>(type: "REAL", nullable: false),
                    LastMonthRevenue = table.Column<double>(type: "REAL", nullable: false),
                    NewAttendees = table.Column<int>(type: "INTEGER", nullable: false),
                    LastMonthAttendees = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalCapacity = table.Column<int>(type: "INTEGER", nullable: false),
                    UsedCapacity = table.Column<int>(type: "INTEGER", nullable: false),
                    LastRemaining = table.Column<int>(type: "INTEGER", nullable: false),
                    EventId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metrics_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_EventId",
                table: "Metrics",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Metrics");
        }
    }
}
