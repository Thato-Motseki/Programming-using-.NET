using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SEIMS.Data;

namespace SEIMS.Services
{
    public class InventoryXmlExporter
    {
        private readonly SEIMSDbContext _context;
        private readonly InventoryDataService _dataService;

        public InventoryXmlExporter(SEIMSDbContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));

            _dataService = new InventoryDataService(context);
        }

        public async Task<string> ExportAsync(
            string outputPath,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException(
                    "An output file path is required.",
                    nameof(outputPath));
            }

            int updatedTrackingNumbers =
                await _dataService.UpdateMissingTrackingNumbersAsync(
                    cancellationToken);

            var inventory = await _context.InventoryItems
                .AsNoTracking()
                .Include(item => item.Supplier)
                .OrderBy(item => item.Name)
                .ToListAsync(cancellationToken);

            var movements = await _context.StockMovements
                .AsNoTracking()
                .OrderBy(movement => movement.Id)
                .ToListAsync(cancellationToken);

            XElement root = new XElement(
                "SEIMSExport",
                new XAttribute(
                    "generatedAt",
                    DateTime.Now.ToString("O", CultureInfo.InvariantCulture)),
                new XAttribute("trackingNumbersUpdated",
                    updatedTrackingNumbers),

                new XElement(
                    "Inventory",
                    inventory.Select(item =>
                        new XElement(
                            "Item",
                            new XAttribute("id", item.Id),
                            new XElement("SKU", item.SKU),
                            new XElement("Name", item.Name),
                            new XElement("Category", item.Category),
                            new XElement("Quantity", item.Quantity),
                            new XElement(
                                "UnitPrice",
                                item.UnitPrice.ToString(
                                    "F2", CultureInfo.InvariantCulture)),
                            new XElement(
                                "TotalValue",
                                item.TotalValue.ToString(
                                    "F2", CultureInfo.InvariantCulture)),
                            new XElement(
                                "Supplier",
                                new XAttribute(
                                    "id", item.SupplierId),
                                item.Supplier?.Name ?? "Unknown")))),

                new XElement(
                    "StockMovements",
                    movements.Select(movement =>
                        new XElement(
                            "Movement",
                            new XAttribute("id", movement.Id),
                            new XElement(
                                "TrackingNumber",
                                movement.TrackingNumber),
                            new XElement(
                                "InventoryItemId",
                                movement.InventoryItemId),
                            new XElement("SKU", movement.SKU),
                            new XElement(
                                "MovementType",
                                movement.MovementType),
                            new XElement("Quantity", movement.Quantity),
                            new XElement(
                                "MovementDate",
                                movement.MovementDate.ToString(
                                    "O", CultureInfo.InvariantCulture)),
                            new XElement("PerformedBy", movement.PerformedBy),
                            new XElement("Notes", movement.Notes ?? string.Empty))))
            );

            XDocument document = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                root);

            string fullPath = Path.GetFullPath(outputPath);

            string? directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using FileStream stream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            await document.SaveAsync(
                stream,
                SaveOptions.None,
                cancellationToken);

            return fullPath;
        }
    }
}