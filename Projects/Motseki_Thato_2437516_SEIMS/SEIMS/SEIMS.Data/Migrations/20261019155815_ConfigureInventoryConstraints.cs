using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEIMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureInventoryConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Suppliers_SupplierId",
                table: "InventoryItems");

            migrationBuilder.AlterColumn<string>(
                name: "SKU",
                table: "InventoryItems",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_InventoryItemId_MovementDate",
                table: "StockMovements",
                columns: new[] { "InventoryItemId", "MovementDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_SKU",
                table: "InventoryItems",
                column: "SKU",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Suppliers_SupplierId",
                table: "InventoryItems",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Suppliers_SupplierId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_InventoryItemId_MovementDate",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_SKU",
                table: "InventoryItems");

            migrationBuilder.AlterColumn<string>(
                name: "SKU",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Suppliers_SupplierId",
                table: "InventoryItems",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
