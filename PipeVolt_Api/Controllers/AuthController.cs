using Microsoft.AspNetCore.Mvc;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PipeVolt_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService auth)
        {
            _authService = auth;
        }


        [HttpPost("Login")]
        public async Task<ActionResult> LoginAsync(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
                return BadRequest(result);
            HttpContext.Session.Remove("JwtToken");
            HttpContext.Session.SetString("JwtToken", result.Token);
            return Ok(result);
        }
        [HttpPost("Register")]
        public async Task<ActionResult> RegisterAsync(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            if (!result.Success)
            {
                if (result.Message.Contains("exists"))
                    return Conflict(result); 
                return BadRequest(result);   
            }
            return Ok(result);
        }
        [HttpPost("Logout")]
        public ActionResult Logout(string username)
        {
            var result = _authService.Logout(username);
            if (!result.Success)
                return BadRequest(result);
            HttpContext.Session.Remove("JwtToken");
            return Ok(result); 
        }

    }
}
