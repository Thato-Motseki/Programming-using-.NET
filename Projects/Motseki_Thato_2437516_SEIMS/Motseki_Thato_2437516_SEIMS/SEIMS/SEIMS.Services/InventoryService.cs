using SEIMS.Core;

namespace SEIMS.Services
{
    public class InventoryService
    {
        private readonly StockMonitor _stockMonitor;

        public InventoryService(StockMonitor stockMonitor)
        {
            _stockMonitor = stockMonitor
                ?? throw new ArgumentNullException(nameof(stockMonitor));
        }

        public void RegisterItem(InventoryItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _stockMonitor.CheckStock(item);
        }

        public void UpdateStock(InventoryItem item, int newQuantity)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (newQuantity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(newQuantity),
                    "Stock quantity cannot be negative.");
            }

            item.UpdateStock(newQuantity);

            _stockMonitor.CheckStock(item);
        }
    }
}