using PipeVolt_DAL.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.IServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        AuthResponseDto Logout(string username);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDTO);
    }
}
