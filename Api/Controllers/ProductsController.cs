using Api.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// CRUD de productos. Todos los endpoints requieren JWT.
    /// Los datos se filtran automáticamente por TenantId del token.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Obtiene todos los productos del tenant del usuario autenticado.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();

            var response = products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                TenantId = p.TenantId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            return Ok(response);
        }

        /// <summary>
        /// Obtiene un producto por ID (solo del tenant del usuario).
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound(new ApiResponse { Success = false, Message = "Producto no encontrado." });

            return Ok(new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                TenantId = product.TenantId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            });
        }

        /// <summary>
        /// Crea un nuevo producto para el tenant del usuario autenticado.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.CreateAsync(
                request.Name, request.Description, request.Price, request.Stock);

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                TenantId = product.TenantId,
                CreatedAt = product.CreatedAt
            });
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _productService.UpdateAsync(
                id, request.Name, request.Description, request.Price, request.Stock);

            if (!success)
                return NotFound(new ApiResponse { Success = false, Message = message });

            return Ok(new ApiResponse { Success = true, Message = message });
        }

        /// <summary>
        /// Elimina un producto por ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _productService.DeleteAsync(id);

            if (!success)
                return NotFound(new ApiResponse { Success = false, Message = message });

            return Ok(new ApiResponse { Success = true, Message = message });
        }
    }
}
