using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.DTOS
{
    public class BrandDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
    }
    public class CreateBrandDto
    {
        [Required(ErrorMessage = "Brand name is required")]
        [StringLength(100, ErrorMessage = "Brand name can't be longer than 100 characters")]
        public string BrandName { get; set; } = string.Empty;
    }
    public class UpdateBrandDto
    {
        [Required(ErrorMessage = "Brand name is required")]
        [StringLength(100, ErrorMessage = "Brand name can't be longer than 100 characters")]
        public string BrandName { get; set; } = string.Empty;
    }
}
