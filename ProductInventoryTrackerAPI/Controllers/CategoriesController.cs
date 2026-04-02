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
    public class CategoriesController : BaseController
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryRepository categoryRepository, ILogger<CategoriesController> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Page>> GetAllCategories([FromQuery] SearchRequestModel model)
        {
            try
            {
                var parameters = FillParamesFromModel(model);
                var list = await _categoryRepository.GetCategoriesAsync(parameters);
                return Ok(BindSearchResult(list, model, "Category List"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                return StatusCode(400, $"Bad Request Error: {ex.Message}");
            }
        }

        [HttpGet("{categorySid}")]
        public async Task<ActionResult<CategoryResponseModel>> GetCategoryBySid(string categorySid)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryBySidAsync(categorySid);
                if (category == null)
                    throw new HttpStatusCodeException(404, $"Category with SID '{categorySid}' not found.");
                return Ok(category);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category {Sid}", categorySid);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("AddCategory")]
        public async Task<ActionResult<CategoryResponseModel>> AddCategory([FromBody] CategoryRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _categoryRepository.AddCategoryAsync(model);
                return CreatedAtAction(nameof(GetCategoryBySid), new { categorySid = response.CategorySid }, response);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpPost("UpdateCategory/{categorySid}")]
        public async Task<ActionResult<CategoryResponseModel>> UpdateCategory(
            [FromRoute] string categorySid,
            [FromBody] CategoryRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _categoryRepository.UpdateCategoryAsync(categorySid, model);
                if (response == null)
                    return NotFound($"Category with SID '{categorySid}' not found.");
                return Ok(response);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {Sid}", categorySid);
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpDelete("{categorySid}")]
        public async Task<IActionResult> DeleteCategory(string categorySid)
        {
            try
            {
                var deleted = await _categoryRepository.DeleteCategoryAsync(categorySid);
                if (!deleted)
                    return NotFound($"Category with SID '{categorySid}' not found.");
                return Ok($"Category with SID '{categorySid}' deleted successfully.");
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {Sid}", categorySid);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("DDLCategory")]
        public async Task<ActionResult<IEnumerable<SelectListItem>>> DDLCategory()
        {
            try
            {
                var result = await _categoryRepository.DDLCategoryAsync();
                if (result == null)
                    return new List<SelectListItem>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category dropdown");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
