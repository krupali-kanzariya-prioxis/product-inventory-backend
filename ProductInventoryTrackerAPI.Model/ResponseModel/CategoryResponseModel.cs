namespace ProductInventoryTrackerAPI.Model.ResponseModel
{
    public class CategoryResponseModel
    {
        public string CategorySid { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}
