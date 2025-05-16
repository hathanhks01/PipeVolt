using Microsoft.AspNetCore.Mvc;
using PipeVolt_DAL.DTOS;
using PipeVolt_BLL.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using PipeVolt_Api.Common;
using Microsoft.AspNetCore.Http;

namespace PipeVolt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IProductService productService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto dto, IFormFile? imageFile)
        {          
            var product = await _productService.CreateProductAsync(dto);
            return Ok(product);
        }



        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductDto productDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, productDto);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
