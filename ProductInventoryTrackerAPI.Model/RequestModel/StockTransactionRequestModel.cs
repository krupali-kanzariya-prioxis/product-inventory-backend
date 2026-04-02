namespace ProductInventoryTrackerAPI.Model.RequestModel
{
    public class StockTransactionRequestModel
    {
        public string ProductSid { get; set; } = null!;
        public string TransactionType { get; set; } = null!;
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
}
