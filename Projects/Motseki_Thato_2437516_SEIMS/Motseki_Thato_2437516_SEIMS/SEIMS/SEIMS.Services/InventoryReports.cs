using System;
using System.Collections.Generic;
using System.Linq;
using SEIMS.Core;

namespace SEIMS.Services
{
    public class InventoryReports
    {
        // Query syntax: order items by total stock value, highest first.
        public IEnumerable<InventoryItem> GetItemsByValueDescending(
            IEnumerable<InventoryItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            var result =
                from item in items
                orderby item.TotalValue descending
                select item;

            return result;
        }

        // Method syntax: order items by quantity, highest first.
        public IEnumerable<InventoryItem> GetItemsByQuantityDescending(
            IEnumerable<InventoryItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            return items.OrderByDescending(item => item.Quantity);
        }

        // Aggregate operator: Sum.
        public decimal GetTotalInventoryValue(
            IEnumerable<InventoryItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            return items.Sum(item => item.TotalValue);
        }

        // Aggregate operator: Average.
        public decimal GetAverageItemValue(
            IEnumerable<InventoryItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            return items.Any()
                ? items.Average(item => item.TotalValue)
                : 0m;
        }

        // Retained from your original implementation.
        public double GetAverageQuantity(
            IEnumerable<InventoryItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            return items.Any()
                ? items.Average(item => item.Quantity)
                : 0d;
        }

        // Group inventory by category.
        public IEnumerable<IGrouping<string, InventoryItem>>
            GroupItemsByCategory(IEnumerable<InventoryItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            return items.GroupBy(item => item.Category);
        }

        // Join inventory items to their suppliers.
        public IEnumerable<string> GetItemsWithSuppliers(
            IEnumerable<InventoryItem> items,
            IEnumerable<Supplier> suppliers)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(suppliers);

            var result =
                from item in items
                join supplier in suppliers
                    on item.SupplierId equals supplier.Id
                select
                    $"{item.Name} | SKU: {item.SKU} | " +
                    $"Category: {item.Category} | " +
                    $"Supplier: {supplier.Name} | " +
                    $"Quantity: {item.Quantity} | " +
                    $"Value: {item.TotalValue:C}";

            return result;
        }
    }
}