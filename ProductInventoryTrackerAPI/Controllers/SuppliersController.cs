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
    public class SuppliersController : BaseController
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(ISupplierRepository supplierRepository, ILogger<SuppliersController> logger)
        {
            _supplierRepository = supplierRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Page>> GetAllSuppliers([FromQuery] SearchRequestModel model)
        {
            try
            {
                var parameters = FillParamesFromModel(model);
                var list = await _supplierRepository.GetSuppliersAsync(parameters);
                return Ok(BindSearchResult(list, model, "Supplier List"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching suppliers");
                return StatusCode(400, $"Bad Request Error: {ex.Message}");
            }
        }

        [HttpGet("{supplierSid}")]
        public async Task<ActionResult<SupplierResponseModel>> GetSupplierBySid(string supplierSid)
        {
            try
            {
                var supplier = await _supplierRepository.GetSupplierBySidAsync(supplierSid);
                if (supplier == null)
                    throw new HttpStatusCodeException(404, $"Supplier with SID '{supplierSid}' not found.");
                return Ok(supplier);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supplier {Sid}", supplierSid);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("AddSupplier")]
        public async Task<ActionResult<SupplierResponseModel>> AddSupplier([FromBody] SupplierRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _supplierRepository.AddSupplierAsync(model);
                return CreatedAtAction(nameof(GetSupplierBySid), new { supplierSid = response.SupplierSid }, response);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding supplier");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpPost("UpdateSupplier/{supplierSid}")]
        public async Task<ActionResult<SupplierResponseModel>> UpdateSupplier(
            [FromRoute] string supplierSid,
            [FromBody] SupplierRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _supplierRepository.UpdateSupplierAsync(supplierSid, model);
                if (response == null)
                    return NotFound($"Supplier with SID '{supplierSid}' not found.");
                return Ok(response);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier {Sid}", supplierSid);
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpDelete("{supplierSid}")]
        public async Task<IActionResult> DeleteSupplier(string supplierSid)
        {
            try
            {
                var deleted = await _supplierRepository.DeleteSupplierAsync(supplierSid);
                if (!deleted)
                    return NotFound($"Supplier with SID '{supplierSid}' not found.");
                return Ok($"Supplier with SID '{supplierSid}' deleted successfully.");
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier {Sid}", supplierSid);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("DDLSupplier")]
        public async Task<ActionResult<IEnumerable<SelectListItem>>> DDLSupplier()
        {
            try
            {
                var result = await _supplierRepository.DDLSupplierAsync();
                if (result == null)
                    return new List<SelectListItem>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supplier dropdown");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
