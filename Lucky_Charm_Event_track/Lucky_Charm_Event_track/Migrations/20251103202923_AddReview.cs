using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyCharmEventtrack.Migrations
{
    /// <inheritdoc />
    public partial class AddReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OverallExperience = table.Column<int>(type: "INTEGER", nullable: false),
                    LikehoodToRecommendTheEvent = table.Column<int>(type: "INTEGER", nullable: false),
                    DidTheEventMeetExpectations = table.Column<int>(type: "INTEGER", nullable: false),
                    WasTheEventWorthTheCost = table.Column<int>(type: "INTEGER", nullable: false),
                    EaseOfCheckinRanking = table.Column<int>(type: "INTEGER", nullable: false),
                    SatisfactionRankingForVenue = table.Column<int>(type: "INTEGER", nullable: false),
                    StaffRankingScore = table.Column<int>(type: "INTEGER", nullable: false),
                    WhatCanBeImproved = table.Column<string>(type: "TEXT", nullable: true),
                    AdditionalComments = table.Column<string>(type: "TEXT", nullable: true),
                    UserAccountID = table.Column<int>(type: "INTEGER", nullable: false),
                    EventID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_UserAccounts_UserAccountID",
                        column: x => x.UserAccountID,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_EventID",
                table: "Reviews",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserAccountID",
                table: "Reviews",
                column: "UserAccountID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");
        }
    }
}
