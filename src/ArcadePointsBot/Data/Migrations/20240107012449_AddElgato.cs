using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcadePointsBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddElgato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElgatoRewardAction",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ChangeValue = table.Column<int>(type: "INTEGER", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    RewardId = table.Column<string>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: true),
                    ActionType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElgatoRewardAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElgatoRewardAction_Rewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "Rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElgatoRewardAction_RewardId",
                table: "ElgatoRewardAction",
                column: "RewardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElgatoRewardAction");
        }
    }
}
