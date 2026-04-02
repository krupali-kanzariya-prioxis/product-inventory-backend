namespace ProductInventoryTrackerAPI.Model.ResponseModel
{
    public class SupplierResponseModel
    {
        public string SupplierSid { get; set; } = null!;
        public string SupplierName { get; set; } = null!;
        public string? ContactEmail { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}
