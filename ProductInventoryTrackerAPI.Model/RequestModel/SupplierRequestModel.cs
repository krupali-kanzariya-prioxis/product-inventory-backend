namespace ProductInventoryTrackerAPI.Model.RequestModel
{
    public class SupplierRequestModel
    {
        public string SupplierName { get; set; } = null!;
        public string? ContactEmail { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
