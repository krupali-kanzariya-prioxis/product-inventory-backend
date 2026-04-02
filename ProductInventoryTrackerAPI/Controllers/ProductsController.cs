using ProductInventoryTrackerAPI.Common;
using ProductInventoryTrackerAPI.Model.CommonModel;
using ProductInventoryTrackerAPI.Model.RequestModel;
using ProductInventoryTrackerAPI.Model.ResponseModel;
using ProductInventoryTrackerAPI.Service.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProductInventoryTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : BaseController
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Page>> GetAllProducts([FromQuery] SearchRequestModel model)
        {
            try
            {
                var parameters = FillParamesFromModel(model);
                var list = await _productRepository.GetProductsAsync(parameters);
                return Ok(BindSearchResult(list, model, "Product List"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return StatusCode(400, $"Bad Request Error: {ex.Message}");
            }
        }

        [HttpGet("{productSid}")]
        public async Task<ActionResult<ProductResponseModel>> GetProductBySid(string productSid)
        {
            try
            {
                var product = await _productRepository.GetProductBySidAsync(productSid);
                if (product == null)
                    throw new HttpStatusCodeException(404, $"Product with SID '{productSid}' not found.");
                return Ok(product);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {Sid}", productSid);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("AddProduct")]
        public async Task<ActionResult<ProductResponseModel>> AddProduct([FromBody] ProductRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _productRepository.AddProductAsync(model);
                return CreatedAtAction(nameof(GetProductBySid), new { productSid = response.ProductSid }, response);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpPost("UpdateProduct/{productSid}")]
        public async Task<ActionResult<ProductResponseModel>> UpdateProduct(
            [FromRoute] string productSid,
            [FromBody] ProductRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _productRepository.UpdateProductAsync(productSid, model);
                if (response == null)
                    return NotFound($"Product with SID '{productSid}' not found.");
                return Ok(response);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Sid}", productSid);
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpDelete("{productSid}")]
        public async Task<IActionResult> DeleteProduct(string productSid)
        {
            try
            {
                var deleted = await _productRepository.DeleteProductAsync(productSid);
                if (!deleted)
                    return NotFound($"Product with SID '{productSid}' not found.");
                return Ok($"Product with SID '{productSid}' deleted successfully.");
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Sid}", productSid);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("LowStock")]
        public async Task<ActionResult<Page>> GetLowStockProducts([FromQuery] SearchRequestModel model)
        {
            try
            {
                var parameters = FillParamesFromModel(model);
                var list = await _productRepository.GetLowStockProductsAsync(parameters);
                return Ok(BindSearchResult(list, model, "Low Stock Product List"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching low stock products");
                return StatusCode(400, $"Bad Request Error: {ex.Message}");
            }
        }

        [HttpGet("DDLProduct")]
        public async Task<ActionResult<IEnumerable<SelectListItem>>> DDLProduct()
        {
            try
            {
                var result = await _productRepository.DDLProductAsync();
                if (result == null)
                    return new List<SelectListItem>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product dropdown");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
