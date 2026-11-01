using Microsoft.EntityFrameworkCore;
using SEIMS.Core;
using SEIMS.Data;

namespace SEIMS.Services
{
    public class InventoryDataService
    {
        private readonly SEIMSDbContext _context;

        public InventoryDataService(SEIMSDbContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<InventoryItem>> GetInventoryAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .AsNoTracking()
                .OrderBy(item => item.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Supplier>> GetSuppliersAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers
                .AsNoTracking()
                .OrderBy(supplier => supplier.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<InventoryItem>> GetInventoryWithSuppliersAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .AsNoTracking()
                .Include(item => item.Supplier)
                .OrderBy(item => item.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<InventoryItem?> GetItemBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                throw new ArgumentException(
                    "SKU is required.", nameof(sku));
            }

            string normalizedSku = sku.Trim().ToUpperInvariant();

            return await _context.InventoryItems
                .Include(item => item.Supplier)
                .FirstOrDefaultAsync(
                    item => item.SKU == normalizedSku,
                    cancellationToken);
        }

        public async Task UpdateItemQuantityAsync(
            int itemId,
            int newQuantity,
            CancellationToken cancellationToken = default)
        {
            if (newQuantity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(newQuantity),
                    "Quantity cannot be negative.");
            }

            InventoryItem? item = await _context.InventoryItems
                .FirstOrDefaultAsync(
                    item => item.Id == itemId,
                    cancellationToken);

            if (item is null)
            {
                throw new InvalidOperationException(
                    $"Inventory item ID {itemId} was not found.");
            }

            item.UpdateStock(newQuantity);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<StockMovement> RecordStockMovementAsync(
            int itemId,
            StockMovementType movementType,
            int quantity,
            string performedBy,
            string? notes = null,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    "Movement quantity must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(performedBy))
            {
                throw new ArgumentException(
                    "The person performing the operation is required.",
                    nameof(performedBy));
            }

            if (!Enum.IsDefined(typeof(StockMovementType), movementType))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(movementType),
                    "Invalid stock movement type.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    cancellationToken);

            InventoryItem? item = await _context.InventoryItems
                .FirstOrDefaultAsync(
                    item => item.Id == itemId,
                    cancellationToken);

            if (item is null)
            {
                throw new InvalidOperationException(
                    $"Inventory item ID {itemId} was not found.");
            }

            int updatedQuantity;

            if (movementType == StockMovementType.Receipt)
            {
                updatedQuantity = checked(item.Quantity + quantity);
            }
            else
            {
                if (quantity > item.Quantity)
                {
                    throw new InvalidOperationException(
                        "The issue quantity exceeds available stock.");
                }

                updatedQuantity = item.Quantity - quantity;
            }

            item.UpdateStock(updatedQuantity);

            var movement = new StockMovement
            {
                InventoryItemId = item.Id,
                SKU = item.SKU,
                MovementType = movementType,
                Quantity = quantity,
                MovementDate = DateTime.Now,
                PerformedBy = performedBy.Trim(),
                Notes = notes?.Trim(),
                TrackingNumber = string.Empty
            };

            _context.StockMovements.Add(movement);

            await _context.SaveChangesAsync(cancellationToken);

            movement.TrackingNumber =
                $"TRK-{movement.MovementDate:yyyyMMdd}-{movement.Id:D6}";

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return movement;
        }

        public async Task<int> UpdateMissingTrackingNumbersAsync(
            CancellationToken cancellationToken = default)
        {
            List<StockMovement> movements =
                await _context.StockMovements
                    .Where(movement =>
                        movement.TrackingNumber == null ||
                        movement.TrackingNumber == string.Empty)
                    .OrderBy(movement => movement.Id)
                    .ToListAsync(cancellationToken);

            foreach (StockMovement movement in movements)
            {
                movement.TrackingNumber =
                    $"TRK-{movement.MovementDate:yyyyMMdd}-{movement.Id:D6}";
            }

            if (movements.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return movements.Count;
        }

        public async Task<List<StockMovement>> GetStockMovementsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.StockMovements
                .AsNoTracking()
                .OrderByDescending(movement => movement.MovementDate)
                .ToListAsync(cancellationToken);
        }
    }
}