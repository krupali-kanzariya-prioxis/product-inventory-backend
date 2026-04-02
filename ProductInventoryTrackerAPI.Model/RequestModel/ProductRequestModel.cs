namespace ProductInventoryTrackerAPI.Model.RequestModel
{
    public class ProductRequestModel
    {
        public string ProductName { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string? Description { get; set; }
        public string? CategorySid { get; set; }
        public string? SupplierSid { get; set; }
        public decimal UnitPrice { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderThreshold { get; set; }
    }
}
