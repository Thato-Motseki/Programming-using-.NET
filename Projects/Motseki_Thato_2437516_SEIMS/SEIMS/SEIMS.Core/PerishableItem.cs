using System;

namespace SEIMS.Core
{
    public class PerishableItem : InventoryItem
    {
        public DateTime ExpiryDate { get; set; }

        public PerishableItem()
        {
        }

        public PerishableItem(
            string sku,
            string name,
            string category,
            int quantity,
            decimal unitPrice,
            DateTime expiryDate)
            : base(sku, name, category, quantity, unitPrice)
        {
            ExpiryDate = expiryDate;
        }

        public bool IsExpired()
        {
            return ExpiryDate.Date < DateTime.Today;
        }

        public override string GetItemDetails()
        {
            string expiryStatus = IsExpired()
                ? "EXPIRED"
                : "Not expired";

            return $"{base.GetItemDetails()} | " +
                   $"Expiry Date: {ExpiryDate:dd MMM yyyy} | " +
                   $"Expiry Status: {expiryStatus}";
        }
    }
}