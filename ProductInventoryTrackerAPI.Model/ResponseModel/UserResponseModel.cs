namespace ProductInventoryTrackerAPI.Model.ResponseModel
{
    public class UserResponseModel
    {
        public string UserSid { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}
