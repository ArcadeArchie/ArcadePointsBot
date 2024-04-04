using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcadePointsBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase().Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Rewards",
                    columns: table => new
                    {
                        Id = table
                            .Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Title = table
                            .Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Cost = table.Column<int>(type: "int", nullable: false),
                        RequireInput = table.Column<bool>(type: "tinyint(1)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Rewards", x => x.Id);
                    }
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder
                .CreateTable(
                    name: "Actions",
                    columns: table => new
                    {
                        Id = table
                            .Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        RewardId = table
                            .Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Duration = table.Column<int>(type: "int", nullable: true),
                        ActionType = table.Column<int>(type: "int", nullable: false),
                        ActionKey = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Actions", x => x.Id);
                        table.ForeignKey(
                            name: "FK_Actions_Rewards_RewardId",
                            column: x => x.RewardId,
                            principalTable: "Rewards",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade
                        );
                    }
                )
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_RewardId",
                table: "Actions",
                column: "RewardId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Actions");

            migrationBuilder.DropTable(name: "Rewards");
        }
    }
}
