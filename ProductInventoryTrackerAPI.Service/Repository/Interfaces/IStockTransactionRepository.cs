using ProductInventoryTrackerAPI.Model.CommonModel;
using ProductInventoryTrackerAPI.Model.RequestModel;
using ProductInventoryTrackerAPI.Model.ResponseModel;

namespace ProductInventoryTrackerAPI.Service.Repository.Interfaces
{
    public interface IStockTransactionRepository
    {
        Task<Page> GetStockTransactionsAsync(Dictionary<string, object> parameters);
        Task<StockTransactionResponseModel?> GetStockTransactionBySidAsync(string transactionSid);
        Task<StockTransactionResponseModel> AddStockTransactionAsync(StockTransactionRequestModel model);
        Task<Page> GetProductTransactionHistoryAsync(string productSid, Dictionary<string, object> parameters);
    }
}
