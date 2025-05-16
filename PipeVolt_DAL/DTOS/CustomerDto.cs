using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name can't be longer than 100 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email can't be longer than 100 characters")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number can't be longer than 20 characters")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Address can't be longer than 200 characters")]
        public string? Address { get; set; }

        public DateOnly RegistrationDate { get; set; }
    }

    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name can't be longer than 100 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email can't be longer than 100 characters")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number can't be longer than 20 characters")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Address can't be longer than 200 characters")]
        public string? Address { get; set; }
    }

    public class UpdateCustomerDto
    {
        [Required(ErrorMessage = "Customer ID is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name can't be longer than 100 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email can't be longer than 100 characters")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number can't be longer than 20 characters")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Address can't be longer than 200 characters")]
        public string? Address { get; set; }
    }
}
