using System;
using System.Collections.Generic;
using System.Linq;

namespace SEIMS.Core
{
    public class ItemWarehouse
    {
        private readonly List<InventoryItem> _items = new();

        // Returns the number of items currently in the warehouse.
        public int Count => _items.Count;

        // Indexer: retrieves an inventory item using its SKU.
        public InventoryItem? this[string sku]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(sku))
                    return null;

                return _items.FirstOrDefault(item =>
                    string.Equals(
                        item.SKU,
                        sku.Trim(),
                        StringComparison.OrdinalIgnoreCase));
            }
        }

        // Adds an item while preventing duplicate SKUs.
        public void AddItem(InventoryItem item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (_items.Any(existing => ReferenceEquals(existing, item)))
            {
                throw new InvalidOperationException(
                    "This inventory item is already in the warehouse.");
            }

            if (_items.Contains(item))
            {
                throw new InvalidOperationException(
                    "This inventory item is already in the warehouse.");
            }

            if (_items.Any(existing =>
                string.Equals(
                    existing.SKU,
                    item.SKU,
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    $"An item with SKU '{item.SKU}' already exists.");
            }

            _items.Add(item);
        }

        // Removes an item using its SKU.
        public bool RemoveItem(string sku)
        {
            InventoryItem? item = this[sku];

            if (item is null)
                return false;

            return _items.Remove(item);
        }

        // Iterator: returns items belonging to a specified category.
        public IEnumerable<InventoryItem> GetItemsByCategory(
            string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                yield break;

            foreach (InventoryItem item in _items)
            {
                if (string.Equals(
                    item.Category,
                    category.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                {
                    yield return item;
                }
            }
        }

        // Returns all items in the warehouse.
        public IEnumerable<InventoryItem> GetAllItems()
        {
            foreach (InventoryItem item in _items)
            {
                yield return item;
            }
        }
    }
}