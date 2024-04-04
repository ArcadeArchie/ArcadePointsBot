using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcadePointsBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class RewardCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Rewards",
                type: "TEXT",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Category", table: "Rewards");
        }
    }
}
