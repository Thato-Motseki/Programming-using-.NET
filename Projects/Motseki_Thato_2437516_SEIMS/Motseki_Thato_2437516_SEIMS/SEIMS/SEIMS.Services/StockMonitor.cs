using SEIMS.Core;

namespace SEIMS.Services
{
    public class StockMonitor
    {
        public event LogHandler? OnStockThresholdCritical;

        public void CheckStock(InventoryItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.Quantity < 5)
            {
                string message =
                    $"CRITICAL STOCK ALERT: {item.Name} " +
                    $"(SKU: {item.SKU}) has only {item.Quantity} " +
                    "units remaining.";

                OnStockThresholdCritical?.Invoke(message);
            }
        }
    }
}