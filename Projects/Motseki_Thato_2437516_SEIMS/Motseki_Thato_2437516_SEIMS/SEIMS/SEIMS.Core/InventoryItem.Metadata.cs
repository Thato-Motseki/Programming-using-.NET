using System;

namespace SEIMS.Core
{
    public partial class InventoryItem : IStorable
    {
        public InventoryItem DeepCopy()
        {
            return new InventoryItem(
                SKU,
                Name,
                Category,
                Quantity,
                UnitPrice)
            {
                Id = Id
            };
        }

        // Checks whether the item has reached the critical stock threshold.
        public bool IsCriticalStock()
        {
            return Quantity < 5;
        }

        public string GetStorageInformation()
        {
            return $"SKU: {SKU} | Category: {Category} | " +
                   $"Quantity in storage: {Quantity}";
        }

        // Updates the quantity while preventing invalid stock levels.
        public void UpdateStock(int newQuantity)
        {
            if (newQuantity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(newQuantity),
                    "Stock quantity cannot be negative.");
            }

            Quantity = newQuantity;
        }   
    }
}