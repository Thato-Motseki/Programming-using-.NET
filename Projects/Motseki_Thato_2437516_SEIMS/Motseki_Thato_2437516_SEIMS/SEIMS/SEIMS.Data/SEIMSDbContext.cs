using Microsoft.EntityFrameworkCore;
using SEIMS.Core;

namespace SEIMS.Data
{
    public class SEIMSDbContext : DbContext
    {
        public SEIMSDbContext(
            DbContextOptions<SEIMSDbContext> options)
            : base(options)
        {
        }

        public DbSet<InventoryItem> InventoryItems =>
            Set<InventoryItem>();

        public DbSet<PerishableItem> PerishableItems =>
            Set<PerishableItem>();

        public DbSet<Supplier> Suppliers =>
            Set<Supplier>();

        public DbSet<StockMovement> StockMovements =>
            Set<StockMovement>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One supplier can supply many inventory items.
            // A supplier cannot be deleted while items reference it.
            modelBuilder.Entity<InventoryItem>()
                .HasOne(item => item.Supplier)
                .WithMany(supplier => supplier.InventoryItems)
                .HasForeignKey(item => item.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Each inventory item must have a unique SKU.
            modelBuilder.Entity<InventoryItem>()
                .HasIndex(item => item.SKU)
                .IsUnique();

            // Store prices using two decimal places.
            modelBuilder.Entity<InventoryItem>()
                .Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            // Each stock movement belongs to an inventory item.
            modelBuilder.Entity<StockMovement>()
                .HasOne<InventoryItem>()
                .WithMany()
                .HasForeignKey(movement => movement.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Improve queries that search movements by item and date.
            modelBuilder.Entity<StockMovement>()
                .HasIndex(movement => new
                {
                    movement.InventoryItemId,
                    movement.MovementDate
                });
        }
    }
}