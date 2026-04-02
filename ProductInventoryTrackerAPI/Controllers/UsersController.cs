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
    public class UsersController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Page>> GetAllUsers([FromQuery] SearchRequestModel model)
        {
            try
            {
                var parameters = FillParamesFromModel(model);
                var list = await _userRepository.GetUsersAsync(parameters);
                return Ok(BindSearchResult(list, model, "User List"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(400, $"Bad Request Error: {ex.Message}");
            }
        }

        [HttpGet("{userSid}")]
        public async Task<ActionResult<UserResponseModel>> GetUserBySid(string userSid)
        {
            try
            {
                var user = await _userRepository.GetUserBySidAsync(userSid);
                if (user == null)
                    throw new HttpStatusCodeException(404, $"User with SID '{userSid}' not found.");
                return Ok(user);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {Sid}", userSid);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("Email/{email}")]
        public async Task<ActionResult<UserResponseModel>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                    throw new HttpStatusCodeException(404, $"User with Email '{email}' not found.");
                return Ok(user);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by email {Email}", email);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("AddUser")]
        public async Task<ActionResult<UserResponseModel>> AddUser([FromBody] UserRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _userRepository.AddUserAsync(model);
                return CreatedAtAction(nameof(GetUserBySid), new { userSid = response.UserSid }, response);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpPost("UpdateUser/{userSid}")]
        public async Task<ActionResult<UserResponseModel>> UpdateUser(
            [FromRoute] string userSid,
            [FromBody] UserRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _userRepository.UpdateUserAsync(userSid, model);
                if (response == null)
                    return NotFound($"User with SID '{userSid}' not found.");
                return Ok(response);
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Sid}", userSid);
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpDelete("{userSid}")]
        public async Task<IActionResult> DeleteUser(string userSid)
        {
            try
            {
                var deleted = await _userRepository.DeleteUserAsync(userSid);
                if (!deleted)
                    return NotFound($"User with SID '{userSid}' not found.");
                return Ok($"User with SID '{userSid}' deleted successfully.");
            }
            catch (HttpStatusCodeException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Sid}", userSid);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("DDLUser")]
        public async Task<ActionResult<IEnumerable<SelectListItem>>> DDLUser()
        {
            try
            {
                var result = await _userRepository.DDLUserAsync();
                if (result == null)
                    return new List<SelectListItem>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user dropdown");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
