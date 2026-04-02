namespace ProductInventoryTrackerAPI.Model.ResponseModel
{
    public class ProductResponseModel
    {
        public string ProductSid { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public string? SupplierName { get; set; }
        public decimal UnitPrice { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderThreshold { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}
