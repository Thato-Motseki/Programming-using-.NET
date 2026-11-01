using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SEIMS.Core;
using SEIMS.Data;
using SEIMS.Services;

internal static class Program
{
    private static readonly string ConnectionString =
        @"Server=LAPTOP-THATO\SQLEXPRESS;" +
        "Database=SEIMS_DB;" +
        "Integrated Security=True;" +
        "TrustServerCertificate=True;";

    private static int _passed;
    private static int _failed;

    private static async Task Main()
    {
        Console.WriteLine("========================================");
        Console.WriteLine(" SEIMS SECTION C INTEGRATION TESTS");
        Console.WriteLine("========================================");

        TestLinQReports();

        await TestDatabaseAndXmlAsync();

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine($"Tests passed: {_passed}");
        Console.WriteLine($"Tests failed: {_failed}");
        Console.WriteLine("========================================");

        if (_failed > 0)
        {
            Environment.ExitCode = 1;
        }
    }

    private static void Check(string name, bool condition)
    {
        if (condition)
        {
            _passed++;
            Console.WriteLine($"PASS: {name}");
        }
        else
        {
            _failed++;
            Console.WriteLine($"FAIL: {name}");
        }
    }

    private static void TestLinQReports()
    {
        Console.WriteLine();
        Console.WriteLine("--- LINQ tests ---");

        var items = new[]
        {
            new InventoryItem(
                "TEST-LAPTOP", "Test Laptop",
                "Electronics", 10, 100m),

            new InventoryItem(
                "TEST-CHAIR", "Test Chair",
                "Furniture", 5, 200m),

            new InventoryItem(
                "TEST-NOTEBOOK", "Test Notebook",
                "Stationery", 15, 10m)
        };

        var reports = new InventoryReports();

        var orderedByValue =
            reports.GetItemsByValueDescending(items).ToList();

        Check(
            "Items are ordered by total value descending",
            orderedByValue[0].SKU == "TEST-LAPTOP" &&
            orderedByValue[1].SKU == "TEST-CHAIR" &&
            orderedByValue[2].SKU == "TEST-NOTEBOOK");

        Check(
            "Total inventory value uses Sum",
            reports.GetTotalInventoryValue(items) == 3000m);

        Check(
            "Average item value uses Average",
            reports.GetAverageItemValue(items) == 1000m);

        Check(
            "Items can be grouped by category",
            reports.GroupItemsByCategory(items).Count() == 3);

        var quantityOrder =
            reports.GetItemsByQuantityDescending(items).ToList();

        Check(
            "Items are ordered by quantity descending",
            quantityOrder[0].SKU == "TEST-NOTEBOOK" &&
            quantityOrder[1].SKU == "TEST-LAPTOP" &&
            quantityOrder[2].SKU == "TEST-CHAIR");

        var suppliers = new[]
        {
            new Supplier
            {
                Id = 1,
                Name = "Test Supplier"
            }
        };

        // In-memory join requires matching supplier IDs.
        items[0].SupplierId = 1;

        var joined = reports.GetItemsWithSuppliers(
            items,
            suppliers).ToList();

        Check(
            "LINQ joins inventory to suppliers",
            joined.Count == 1 &&
            joined[0].Contains("Test Supplier"));
    }

    private static async Task TestDatabaseAndXmlAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- SQL Server and XML tests ---");

        var options = new DbContextOptionsBuilder<SEIMSDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        string testToken = Guid.NewGuid()
            .ToString("N")[..8]
            .ToUpperInvariant();

        string testSku = $"T{testToken}";
        string supplierName = $"SEIMS TEST SUPPLIER {testToken}";

        int supplierId = 0;
        int itemId = 0;
        string exportPath = Path.Combine(
            Path.GetTempPath(),
            $"SEIMS-TEST-{testToken}.xml");

        try
        {
            await using (var context = new SEIMSDbContext(options))
            {
                bool canConnect =
                    await context.Database.CanConnectAsync();

                Check(
                    "Connects to SQL Server database SEIMS_DB",
                    canConnect);

                if (!canConnect)
                {
                    return;
                }

                var supplier = new Supplier
                {
                    Name = supplierName,
                    ContactPerson = "Test Contact",
                    PhoneNumber = "0000000000",
                    Email = $"test-{testToken}@example.com",
                    Address = "Test Address"
                };

                context.Suppliers.Add(supplier);
                await context.SaveChangesAsync();

                supplierId = supplier.Id;

                var item = new InventoryItem(
                    testSku,
                    "SEIMS Integration Test Item",
                    "Testing",
                    10,
                    25m)
                {
                    SupplierId = supplierId
                };

                context.InventoryItems.Add(item);
                await context.SaveChangesAsync();

                itemId = item.Id;

                Check(
                    "Creates test supplier and inventory item",
                    supplierId > 0 && itemId > 0);
            }

            await using (var context = new SEIMSDbContext(options))
            {
                var dataService = new InventoryDataService(context);

                var loadedItem =
                    await dataService.GetItemBySkuAsync(testSku);

                Check(
                    "Reads the test item from SQL Server",
                    loadedItem is not null &&
                    loadedItem.SKU == testSku &&
                    loadedItem.Quantity == 10);

                var inventory =
                    await dataService.GetInventoryAsync();

                Check(
                    "Retrieves live inventory records",
                    inventory.Any(item => item.SKU == testSku));

                var suppliers =
                    await dataService.GetSuppliersAsync();

                Check(
                    "Retrieves live supplier records",
                    suppliers.Any(supplier =>
                        supplier.Id == supplierId));

                if (loadedItem is not null)
                {
                    await dataService.RecordStockMovementAsync(
                        itemId,
                        StockMovementType.Receipt,
                        5,
                        "Integration Test",
                        "Test receipt");

                    var afterReceipt =
                        await dataService.GetItemBySkuAsync(testSku);

                    Check(
                        "Receipt increases stock from 10 to 15",
                        afterReceipt?.Quantity == 15);
                }
            }

            await using (var context = new SEIMSDbContext(options))
            {
                var dataService = new InventoryDataService(context);

                await dataService.RecordStockMovementAsync(
                    itemId,
                    StockMovementType.Issue,
                    3,
                    "Integration Test",
                    "Test issue");

                var afterIssue =
                    await dataService.GetItemBySkuAsync(testSku);

                Check(
                    "Issue reduces stock from 15 to 12",
                    afterIssue?.Quantity == 12);

                var movements =
                    await dataService.GetStockMovementsAsync();

                var testMovements = movements
                    .Where(movement =>
                        movement.InventoryItemId == itemId)
                    .ToList();

                Check(
                    "Stock movements receive tracking numbers",
                    testMovements.Count == 2 &&
                    testMovements.All(movement =>
                        !string.IsNullOrWhiteSpace(
                            movement.TrackingNumber)));
            }

            await using (var context = new SEIMSDbContext(options))
            {
                var exporter = new InventoryXmlExporter(context);

                string generatedPath =
                    await exporter.ExportAsync(exportPath);

                Check(
                    "XML exporter creates an output file",
                    File.Exists(generatedPath));

                XDocument document = XDocument.Load(generatedPath);

                Check(
                    "Generated XML contains inventory data",
                    document.Root?
                        .Element("Inventory")?
                        .Elements("Item")
                        .Any(element =>
                            (string?)element.Element("SKU") == testSku)
                    == true);

                Check(
                    "Generated XML contains stock movements",
                    document.Root?
                        .Element("StockMovements")?
                        .Elements("Movement")
                        .Any(element =>
                            (int?)element.Element("InventoryItemId")
                            == itemId)
                    == true);
            }
        }
        catch (Exception ex)
        {
            _failed++;
            Console.WriteLine(
                $"FAIL: Database/XML integration test: {ex.Message}");
            Console.WriteLine(ex);
        }
        finally
        {
            // Remove only the records created by this test.
            try
            {
                await using var context =
                    new SEIMSDbContext(options);

                var movements = await context.StockMovements
                    .Where(movement =>
                        movement.InventoryItemId == itemId)
                    .ToListAsync();

                context.StockMovements.RemoveRange(movements);

                var item = await context.InventoryItems
                    .FirstOrDefaultAsync(
                        candidate => candidate.Id == itemId);

                if (item is not null)
                {
                    context.InventoryItems.Remove(item);
                }

                var supplier = await context.Suppliers
                    .FirstOrDefaultAsync(
                        candidate => candidate.Id == supplierId);

                if (supplier is not null)
                {
                    context.Suppliers.Remove(supplier);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception cleanupException)
            {
                Console.WriteLine(
                    "WARNING: Test cleanup did not finish: " +
                    cleanupException.Message);
            }

            try
            {
                if (File.Exists(exportPath))
                {
                    File.Delete(exportPath);
                }
            }
            catch (IOException)
            {
                Console.WriteLine(
                    "WARNING: Could not remove the temporary XML file.");
            }
        }
    }
}