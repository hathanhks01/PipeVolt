using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    public class ProductCategoryDto
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;
        [StringLength(255)]
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
    }

    public class CreateProductCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name can't exceed 100 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        [StringLength(500, ErrorMessage = "Description can't exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class UpdateProductCategoryDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name can't exceed 100 characters")]
        public string CategoryName { get; set; } = string.Empty;
        [StringLength(255)]
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        [StringLength(500, ErrorMessage = "Description can't exceed 500 characters")]
        public string? Description { get; set; }
    }
}
