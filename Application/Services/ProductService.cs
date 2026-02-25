using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ITenantService _tenantService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            ITenantService tenantService,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            _logger.LogInformation("Fetching all products for tenant '{TenantId}'", tenantId);
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<Product> CreateAsync(string name, string description, decimal price, int stock)
        {
            var tenantId = _tenantService.GetCurrentTenantId();

            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Stock = stock,
                TenantId = tenantId
            };

            var created = await _productRepository.CreateAsync(product);
            _logger.LogInformation("Product '{ProductName}' created for tenant '{TenantId}'", name, tenantId);
            return created;
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, string name, string description, decimal price, int stock)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return (false, "Producto no encontrado.");

            product.Name = name;
            product.Description = description;
            product.Price = price;
            product.Stock = stock;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            _logger.LogInformation("Product '{ProductId}' updated", id);
            return (true, "Producto actualizado exitosamente.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return (false, "Producto no encontrado.");

            await _productRepository.DeleteAsync(id);
            _logger.LogInformation("Product '{ProductId}' deleted", id);
            return (true, "Producto eliminado exitosamente.");
        }
    }
}
