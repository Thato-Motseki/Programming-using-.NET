public class InventoryItemDemo
{
    public string SKU { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }

    public InventoryItemDemo(
        string sku,
        string name,
        int quantity)
    {
        SKU = sku;
        Name = name;
        Quantity = quantity;
    }

    public string GetItemDetails()
    {
        return $"SKU: {SKU} | Name: {Name} | Quantity: {Quantity}";
    }
}
