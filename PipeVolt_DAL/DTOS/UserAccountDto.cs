using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    using System.ComponentModel.DataAnnotations;

    namespace PipeVolt_DAL.DTOS
    {
        public class UserAccountDto
        {
            public int UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public int UserType { get; set; }
            public int? EmployeeId { get; set; }
            public int? CustomerId { get; set; }
            public int? Status { get; set; }
        }

        public class CreateUserAccountDto
        {
            [Required(ErrorMessage = "Username is required")]
            [StringLength(50, ErrorMessage = "Username can't be longer than 50 characters")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [StringLength(255, ErrorMessage = "Password can't be longer than 255 characters")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "User type is required")]
            public int UserType { get; set; }

            public int? EmployeeId { get; set; }
            public int? CustomerId { get; set; }
            public int? Status { get; set; }
        }

        public class UpdateUserAccountDto
        {
            [Required(ErrorMessage = "Username is required")]
            [StringLength(50, ErrorMessage = "Username can't be longer than 50 characters")]
            public string Username { get; set; } = string.Empty;

            [StringLength(255, ErrorMessage = "Password can't be longer than 255 characters")]
            public string? Password { get; set; } // Password có thể không bắt buộc khi cập nhật

            [Required(ErrorMessage = "User type is required")]
            public int UserType { get; set; }

            public int? EmployeeId { get; set; }
            public int? CustomerId { get; set; }
            public int? Status { get; set; }
        }
    }
}
