using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcadePointsBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexCol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Actions",
                type: "int",
                nullable: false,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Index", table: "Actions");
        }
    }
}
