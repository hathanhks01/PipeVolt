using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_BLL.Services;
using PipeVolt_DAL.DTOS.PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountController : ControllerBase
    {
        private readonly IUserAccountService _userAccountService;

        public UserAccountController(IUserAccountService userAccountService, ILoggerService loggerService)
        {
            _userAccountService = userAccountService;
        }

        // GET: api/useraccounts
        [HttpGet]
        [Route("GetList")]
        public async Task<ActionResult<List<UserAccountDto>>> GetAllUserAccounts()
        {
            try
            {
                var userAccounts = await _userAccountService.GetAllUserAccountsAsync();
                return Ok(userAccounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/useraccounts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserAccountDto>> GetUserAccount(int id)
        {
            try
            {
                var userAccount = await _userAccountService.GetUserAccountByIdAsync(id);
                return Ok(userAccount);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/useraccounts/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserAccountDto>> GetUserAccountByUsername(string username)
        {
            try
            {
                var userAccount = await _userAccountService.GetUserAccountByUsernameAsync(username);
                return Ok(userAccount);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/useraccounts
        [HttpPost]
        public async Task<ActionResult<UserAccountDto>> CreateUserAccount([FromBody] CreateUserAccountDto createUserAccountDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userAccount = await _userAccountService.AddUserAccountAsync(createUserAccountDto);
                return CreatedAtAction(nameof(GetUserAccount), new { id = userAccount.UserId }, userAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/useraccounts/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<UserAccountDto>> UpdateUserAccount(int id, [FromBody] UpdateUserAccountDto updateUserAccountDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userAccount = await _userAccountService.UpdateUserAccountAsync(id, updateUserAccountDto);
                return Ok(userAccount);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/useraccounts/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserAccount(int id)
        {
            try
            {
                var result = await _userAccountService.DeleteUserAccountAsync(id);
                if (!result)
                {
                    return NotFound("User account not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
