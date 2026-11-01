using System;

namespace SEIMS.Core
{
    public enum StockMovementType
    {
        Receipt,
        Issue
    }

    public class StockMovement
    {
        public int Id { get; set; }

        public int InventoryItemId { get; set; }

        public string SKU { get; set; } = string.Empty;

        public string TrackingNumber { get; set; } = string.Empty;

        public StockMovementType MovementType { get; set; }

        public int Quantity { get; set; }

        public DateTime MovementDate { get; set; } = DateTime.Now;

        public string PerformedBy { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}