using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEIMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovementRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_InventoryItems_InventoryItemId",
                table: "StockMovements",
                column: "InventoryItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_InventoryItems_InventoryItemId",
                table: "StockMovements");
        }
    }
}
