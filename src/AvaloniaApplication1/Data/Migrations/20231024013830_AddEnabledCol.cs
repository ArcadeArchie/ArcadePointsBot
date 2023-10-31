using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvaloniaApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnabledCol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Rewards",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Rewards");
        }
    }
}
