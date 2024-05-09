using StoreApi.Models;
using StoreApi.Repositories;
using Microsoft.Extensions.Configuration;

namespace StoreApiTests
{
    public class ProductsTest
    {
        private IConfiguration _configuration;
        private IProductRepository _productRepository;

        [SetUp]
        public void Setup()
        {
            _configuration = new ConfigurationBuilder()
                       .AddJsonFile("appsettings.json")
                       .Build();
            _productRepository = new ProductRepository(_configuration);
        }

        [Test]
        public async Task AddProductAsync_ValidProduct_ReturnsProduct()
        {
            var product = new Product
            {
                Uuid = Guid.NewGuid(),
                Name = "Test Product",
                ImageUrl = "test_image.jpg",
                Price = 50000,
                Description = "Test description",
                Category = Guid.Parse("4a8c74b4-cf8e-4fbf-81a2-3d11e1e37d18")
            };


            var result = await _productRepository.AddProductAsync(product);

            Assert.NotNull(result);
            Assert.AreEqual(product.Uuid, result.Uuid);
        }

        [Test]
        public async Task GetProductByIdAsync_NonExistingId_ReturnsNull()
        {
            var nonExistingProductId = Guid.NewGuid();

            var result = await _productRepository.GetProductByIdAsync(nonExistingProductId);

            Assert.Null(result);
        }

        [Test]
        public async Task UpdateProductAsync_ValidProduct_ReturnsOne()
        {
            var existingProduct = new Product
            {
                Uuid = Guid.NewGuid(),
                Name = "Existing Product",
                ImageUrl = "update_image.jpg",
                Price = 1000000,
                Description = "Update description"
            };

            var result = await _productRepository.UpdateProductAsync(existingProduct);

            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task GetProductByCategory()
        {
            Guid category = Guid.Parse("4a8c74b4-cf8e-4fbf-81a2-3d11e1e37d18");
            var result = await _productRepository.GetProductByCategoryAsync(category);
            Assert.NotNull(result);
        }
    }

}