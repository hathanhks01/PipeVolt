using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public double? SellingPrice { get; set; }
        public string? Unit { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public string CategoryName { get; set; } = null!; // Thêm thông tin của Category
        public string BrandName { get; set; } = null!;    // Thêm thông tin của Brand
    }
    public class InventoryProductDto
    {
        public ProductDto Product { get; set; }
        public int Quantity { get; set; }
    }
    public class CreateProductDto
    {
        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = null!;

        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public double? SellingPrice { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        public string? Description { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
    public class UpdateProductDto
    {
        [Required]
        public int ProductId { get; set; }

        [StringLength(50)]
        public string ProductCode { get; set; } = null!;

        [StringLength(200)]
        public string ProductName { get; set; } = null!;

        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public double? SellingPrice { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        public string? Description { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
    }


}
