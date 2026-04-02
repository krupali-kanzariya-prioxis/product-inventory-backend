namespace ProductInventoryTrackerAPI.Model.ResponseModel
{
    public class StockTransactionResponseModel
    {
        public string StockTransactionSid { get; set; } = null!;
        public string ProductSid { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string TransactionType { get; set; } = null!;
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}
