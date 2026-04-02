using ProductInventoryTrackerAPI.Common;
using ProductInventoryTrackerAPI.Model.CommonModel;
using ProductInventoryTrackerAPI.Model.RequestModel;
using ProductInventoryTrackerAPI.Model.ResponseModel;
using ProductInventoryTrackerAPI.Service.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ProductInventoryTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTransactionsController : BaseController
    {
        private readonly IStockTransactionRepository _transactionRepository;
        private readonly ILogger<StockTransactionsController> _logger;

        public StockTransactionsController(IStockTransactionRepository transactionRepository, ILogger<StockTransactionsController> logger)
        {
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Page>> GetAllTransactions([FromQuery] SearchRequestModel model)
        {
            try
            {
                var parameters = FillParamesFromModel(model);
                var list = await _transactionRepository.GetStockTransactionsAsync(parameters);
                return Ok(BindSearchResult(list, model, "Stock Transaction List"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions");
                return StatusCode(400, $"Bad Request Error: {ex.Message}");
            }
        }

        [HttpGet("{transactionSid}")]
        public async Task<ActionResult<StockTransactionResponseModel>> GetTransactionBySid(string transactionSid)
        {
            try
            {
                var transaction = await _transactionRepository.GetStockTransactionBySidAsync(transactionSid);
                if (transaction == null)
                    throw new HttpStatusCodeException(404, $"Transaction with SID '{transactionSid}' not found.");
                return Ok(transaction);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transaction {Sid}", transactionSid);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("AddTransaction")]
        public async Task<ActionResult<StockTransactionResponseModel>> AddTransaction([FromBody] StockTransactionRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _transactionRepository.AddStockTransactionAsync(model);
                return CreatedAtAction(nameof(GetTransactionBySid), new { transactionSid = response.StockTransactionSid }, response);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding transaction");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpGet("ProductHistory/{productSid}")]
        public async Task<ActionResult<Page>> GetProductTransactionHistory(
            [FromRoute] string productSid,
            [FromQuery] SearchRequestModel model)
        {
            try
            {
                var parameters = FillParamesFromModel(model);
                var list = await _transactionRepository.GetProductTransactionHistoryAsync(productSid, parameters);
                return Ok(BindSearchResult(list, model, $"Product {productSid} Transaction History"));
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product history {ProductSid}", productSid);
                return StatusCode(400, $"Bad Request Error: {ex.Message}");
            }
        }
    }
}
