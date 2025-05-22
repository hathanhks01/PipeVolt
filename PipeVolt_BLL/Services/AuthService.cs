using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PipeVolt_Api.Common.Repository;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserAccountRepository IUserAccountRepository;
        private readonly IGenericRepository<UserAccount> _UserGenericRepository;
        private readonly ICustomerRepository ICustomerRepository;
        private readonly IGenericRepository<Customer> _CusGenericRepository;
        private readonly ILoggerService _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor; 
        public AuthService(
            IUserAccountRepository UserAccountRepository,
            ICustomerRepository CustomerRepository,
            IGenericRepository<Customer> CusGenericRepository,
            ILoggerService logger,
            IConfiguration configuration,
             IHttpContextAccessor httpContextAccessor)
        {
            IUserAccountRepository = UserAccountRepository;
            ICustomerRepository = CustomerRepository;
            _CusGenericRepository = CusGenericRepository;
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await IUserAccountRepository.FindByUsernameAsync(loginDto.Username);

                if (user == null)
                {
                    _logger.LogWarning($"Login failed: User {loginDto.Username} not found");
                    return new AuthResponseDto { Success = false, Message = "Invalid username or password" };
                }
                if (user.Status != 1)
                {
                    _logger.LogWarning($"Login attempt with inactive account: {loginDto.Username}");
                    return new AuthResponseDto { Success = false, Message = "Account is inactive or suspended" };
                }
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                {
                    _logger.LogWarning($"Login failed: Invalid password for user {loginDto.Username}");
                    return new AuthResponseDto { Success = false, Message = "Invalid username or password" };
                }
                var token = GenerateJwtToken(user);
                _logger.LogInformation($"User {loginDto.Username} logged in successfully");
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    Username = user.Username,
                    UserType = user.UserType,
                    UserId = user.UserId
                };

            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred during login", ex);
                return new AuthResponseDto { Success = false, Message = "An error occurred during login" };
            }
        }
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDTO)
        {
            try
            {
                var existingUser = await IUserAccountRepository.FindByUsernameAsync(registerDTO.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Registration failed: Username {registerDTO.Username} already exists");
                    return new AuthResponseDto { Success = false, Message = "Username already exists" };
                }
               
                string HashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password);
                string code = await ICustomerRepository.RenderCodeAsync();
                var customer = new Customer
                {
                    CustomerCode = code,
                    CustomerName = registerDTO.Username,
                    RegistrationDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                };
                await _CusGenericRepository.Create(customer);
                var user = new UserAccount
                {
                    Username = registerDTO.Username,
                    Password = HashedPassword,
                    UserType = 2,
                    CustomerId = customer.CustomerId,
                    Status = 1
                };
                await _UserGenericRepository.Create(user);
                _logger.LogInformation($"User {registerDTO.Username} registered successfully");
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Registration successful",
                    Username = user.Username,
                    UserType = user.UserType,
                    UserId = user.UserId
                };

            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred during registration:", ex);
                return new AuthResponseDto { Success = false, Message = "An error occurred during registration" };
            }
        }
        public AuthResponseDto Logout(string username)
        {
            _logger.LogInformation($"User {username} logged out");
            return new AuthResponseDto
            {
                Success = true,
                Message = "Logout successful"
            };
        }

        private string GenerateJwtToken(UserAccount user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var jwtExpireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"]);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim("userType", user.UserType.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                   issuer: jwtIssuer,
                   audience: jwtAudience,
                   claims: claims,
                   expires: DateTime.UtcNow.AddMinutes(jwtExpireMinutes),
                   signingCredentials: credentials
               );   

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private bool IsValidUserType(int userType)
        {
            return userType >= 0 && userType <= 2;
        }
    }
}
