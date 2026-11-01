using System;
using System.Diagnostics;

namespace SEIMS.Core
{
    public partial class InventoryItem : IEquatable<InventoryItem>, IStorable
    {
        // Private fields demonstrate encapsulation.
        private int _id;
        private string _sku = string.Empty;
        private string _name = string.Empty;
        private string _category = string.Empty;
        private int _quantity;
        private decimal _unitPrice;

        // Public properties control access to the private fields.
        public int Id
        {
            get => _id;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(
                        nameof(value), "ID cannot be negative.");

                _id = value;
            }
        }

        public int SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        public string SKU
        {
            get => _sku;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(
                        "SKU cannot be empty.", nameof(value));

                _sku = value.Trim().ToUpperInvariant();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(
                        "Item name cannot be empty.", nameof(value));

                _name = value.Trim();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(
                        "Category cannot be empty.", nameof(value));

                _category = value.Trim();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(
                        nameof(value), "Quantity cannot be negative.");

                _quantity = value;
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(
                        nameof(value), "Unit price cannot be negative.");

                _unitPrice = value;
            }
        }

        // Calculates the total value of the stock for this item.
        public decimal TotalValue => Quantity * UnitPrice;

        // Parameterless constructor supports object creation and EF Core.
        public InventoryItem()
        {
        }

        // Parameterized constructor initializes a complete inventory item.
        public InventoryItem(
            string sku,
            string name,
            string category,
            int quantity,
            decimal unitPrice)
        {
            SKU = sku;
            Name = name;
            Category = category;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        // Virtual allows derived classes to provide their own implementation.
        public virtual string GetItemDetails()
        {
            return $"SKU: {SKU} | Name: {Name} | Category: {Category} | " +
                   $"Quantity: {Quantity} | Unit Price: {UnitPrice:C} | " +
                   $"Total Value: {TotalValue:C}";
        }


        // Compares inventory items using their stable business identifier.
        public bool Equals(InventoryItem? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Id == 0 || other.Id == 0)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return obj is InventoryItem other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id == 0
                ? System.Runtime.CompilerServices.RuntimeHelpers
                    .GetHashCode(this)
                : Id.GetHashCode();
        }

        string IStorable.GetStorageInformation()
        {
            return $"SKU: {SKU} | Name: {Name} | Category: {Category} | " +
                   $"Quantity: {Quantity} | Unit Price: {UnitPrice:C}";
        }

        ~InventoryItem()
        {
            Debug.WriteLine($"InventoryItem finalized: SKU = {_sku}");
        }
    }
}