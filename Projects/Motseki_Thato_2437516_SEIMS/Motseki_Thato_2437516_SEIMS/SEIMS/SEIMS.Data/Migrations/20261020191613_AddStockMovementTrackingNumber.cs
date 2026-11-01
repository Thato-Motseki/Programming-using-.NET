using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEIMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovementTrackingNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrackingNumber",
                table: "StockMovements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackingNumber",
                table: "StockMovements");
        }
    }
}
