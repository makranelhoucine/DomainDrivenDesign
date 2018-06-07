﻿using Infrastructure.Dapper.Repository;
using Infrastructure.UnitOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Dapper.Tests.Repository
{
    [TestClass]
    public class DapperRepositoryShould
    {
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class ProductRepository : DapperRepository<Product>
        {
            public ProductRepository(IUnitOfWork unitOfWork)
                : base (unitOfWork)
            {
            }

            protected override ParametrizedQuery GetAllCommand()
            {
                return new ParametrizedQuery("SELECT * FROM Product");
            }

            protected override ParametrizedQuery GetByIdCommand(long id)
            {
                return new ParametrizedQuery(
                    "SELECT * FROM Product WHERE Id = @Id",
                    new { Id = id });
            }

            protected override ParametrizedQuery SaveCommand(Product aggregateRoot)
            {
                return new ParametrizedQuery(
                    "INSERT INTO Product(id, Name, Description) VALUES (@Id, @Name, @Description)",
                    new { aggregateRoot.Id, aggregateRoot.Name, aggregateRoot.Description });
            }
        }

        [TestMethod]
        public void RetrieveProduct_When_ClaimingProduct()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id=1,Name="Product1", Description="This is the first product" },
                new Product { Id=2,Name="Product2", Description="This is the second product" }
            };
            var db = new InMemoryDatabase();
            db.Insert(products);

            using (var connection = db.OpenConnection())
            {
                var unitOfWork = new UnitOfWork.UnitOfWork(connection);
                ProductRepository productRepository = new ProductRepository(unitOfWork);

                // Act
                var result = productRepository.GetById(1);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Id);
                Assert.AreEqual("Product1", result.Name);
                Assert.AreEqual("This is the first product", result.Description);
            }
        }

        [TestMethod]
        public void SaveProduct_When_NewProductInserted()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id=1, Name = "Product1", Description = "This is the first product" },
                new Product { Id=2, Name = "Product2", Description = "This is the second product" }
            };
            var db = new InMemoryDatabase();
            db.Insert(products);

            using (var connection = db.OpenConnection())
            {
                var unitOfWork = new UnitOfWork.UnitOfWork(connection);
                ProductRepository productRepository = new ProductRepository(unitOfWork);
                var product = new Product { Id = 3, Name="Product3", Description = "This is the third product" };

                // Act
                productRepository.Save(product);

                // Assert
                var allProducts = db.GetAll<Product>();
                Assert.AreEqual(3, allProducts.Count());

                var thirdProduct = allProducts.ToList()[2];
                Assert.IsNotNull(thirdProduct);

                Assert.AreEqual(3, thirdProduct.Id);
                Assert.AreEqual("Product3", thirdProduct.Name);
                Assert.AreEqual("This is the third product", thirdProduct.Description);
            }
        }

        [TestMethod]
        public void ListAllProducts_When_ClaimingAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id=1, Name = "Product1", Description = "This is the first product" },
                new Product { Id=2, Name = "Product2", Description = "This is the second product" }
            };
            var db = new InMemoryDatabase();
            db.Insert(products);

            using (var connection = db.OpenConnection())
            {
                var unitOfWork = new UnitOfWork.UnitOfWork(connection);
                ProductRepository productRepository = new ProductRepository(unitOfWork);
                
                // Act
                var allProducts = productRepository.GetAll();

                // Assert
                Assert.AreEqual(2, allProducts.Count());
            }
        }
    }
}
