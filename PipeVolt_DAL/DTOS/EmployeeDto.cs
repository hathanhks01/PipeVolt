using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    // EmployeeDto.cs
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Position { get; set; }
        public DateOnly? HireDate { get; set; }
    }

    // CreateEmployeeDto.cs
    public class CreateEmployeeDto
    {
        [Required] public string EmployeeCode { get; set; } = string.Empty;
        [Required] public string EmployeeName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Position { get; set; }
    }

    // UpdateEmployeeDto.cs
    public class UpdateEmployeeDto
    {
        [Required] public int EmployeeId { get; set; }
        [Required] public string EmployeeCode { get; set; } = string.Empty;
        [Required] public string EmployeeName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Position { get; set; }
    }

}
