using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Grocery.Shopping.API.Domain.Entities;
using Grocery.Shopping.API.Domain.Services;
using Grocery.Shopping.API.Dtos;
using Grocery.Shopping.API.Enums;
using Grocery.Shopping.API.Infra.Mongo;
using Grocery.Shopping.API.Tests.Helpers;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace Grocery.Shopping.API.Tests.Services
{
    public class EstoqueServiceTests
    {
        private readonly IMongoDbContext _mockContext;
        private readonly EstoqueService _sut;

        public EstoqueServiceTests()
        {
            _mockContext = Substitute.For<IMongoDbContext>();
            _sut = new EstoqueService(_mockContext);
        }

        [Fact]
        public async Task AdicionarEstoqueAsync_WithNullProduto_ThrowsArgumentNullException()
        {
            // Arrange
            var request = new AdicionarEstoqueRequestDto
            {
                Produto = null!,
                Movimentacao = new MovimentacaoEstoqueCreateDto { QuantidadeUnidades = 10 }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AdicionarEstoqueAsync(request));
        }

        [Fact]
        public async Task AdicionarEstoqueAsync_WithNullMovimentacao_ThrowsArgumentNullException()
        {
            // Arrange
            var request = new AdicionarEstoqueRequestDto
            {
                Produto = new ProdutoUpsertDto { Nome = "Arroz" },
                Movimentacao = null!
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AdicionarEstoqueAsync(request));
        }

        [Fact]
        public async Task AdicionarEstoqueAsync_WithInvalidQuantity_ThrowsArgumentException()
        {
            // Arrange
            var request = new AdicionarEstoqueRequestDto
            {
                Produto = new ProdutoUpsertDto { Nome = "Arroz" },
                Movimentacao = new MovimentacaoEstoqueCreateDto { QuantidadeUnidades = 0 }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AdicionarEstoqueAsync(request));
        }

        [Fact]
        public async Task AdicionarEstoqueAsync_WhenProductIsNew_InsertsProductAndMovementAndCalculatesStock()
        {
            // Arrange
            var request = new AdicionarEstoqueRequestDto
            {
                Produto = new ProdutoUpsertDto
                {
                    Nome = "Arroz",
                    Marca = "Camil",
                    UnidadeMedida = "kg",
                    Categoria = CategoriaProduto.Graos,
                    CodigoBarras = "78910"
                },
                Movimentacao = new MovimentacaoEstoqueCreateDto
                {
                    Tipo = TipoMovimentoEstoque.Entrada,
                    QuantidadeUnidades = 5,
                    Motivo = "Compra mensal",
                    DataVencimento = DateTime.UtcNow.AddMonths(6)
                }
            };

            // Setup MongoDB Mocks
            var mockProdutosCol = MongoDbTestHelpers.CreateMockCollection(new List<Produto>()); // Empty (new product)
            var mockMovCol = MongoDbTestHelpers.CreateMockCollection(new List<MovimentacaoEstoque>()); // Empty before insert

            _mockContext.Produtos.Returns(mockProdutosCol);
            _mockContext.MovimentacoesEstoque.Returns(mockMovCol);

            // Act
            var result = await _sut.AdicionarEstoqueAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.ProdutoId.Should().NotBeNullOrWhiteSpace();
            result.MovimentacaoId.Should().NotBeNullOrWhiteSpace();
            result.QuantidadeTotalProdutoAposEntrada.Should().Be(0); // Because we mock MovimentacoesEstoque as returning empty list during calculation

            // Verify inserts
            await mockProdutosCol.Received(1).InsertOneAsync(
                Arg.Is<Produto>(p => p.Nome == "Arroz" && p.Marca == "Camil"),
                Arg.Any<InsertOneOptions>(),
                Arg.Any<CancellationToken>()
            );

            await mockMovCol.Received(1).InsertOneAsync(
                Arg.Is<MovimentacaoEstoque>(m => m.QuantidadeUnidades == 5 && m.Tipo == TipoMovimentoEstoque.Entrada),
                Arg.Any<InsertOneOptions>(),
                Arg.Any<CancellationToken>()
            );
        }

        [Fact]
        public async Task AdicionarEstoqueAsync_WhenProductExistsById_UpdatesProductAndInsertsMovementAndCalculatesStock()
        {
            // Arrange
            var existingProduto = new Produto
            {
                Id = "existing-id",
                Nome = "Old Name",
                Marca = "Old Brand",
                CodigoBarras = "12345"
            };

            var request = new AdicionarEstoqueRequestDto
            {
                Produto = new ProdutoUpsertDto
                {
                    Id = "existing-id",
                    Nome = "New Name",
                    Marca = "New Brand",
                    CodigoBarras = "12345"
                },
                Movimentacao = new MovimentacaoEstoqueCreateDto
                {
                    Tipo = TipoMovimentoEstoque.Entrada,
                    QuantidadeUnidades = 3
                }
            };

            var mockProdutosCol = MongoDbTestHelpers.CreateMockCollection(new List<Produto> { existingProduto });
            
            // To simulate calculating the new balance, we return the past movement and mock that the query will find it.
            // Wait, we need to test that CalcularSaldoProdutoAsync gets all movements and sums them up.
            var existingMovement = new MovimentacaoEstoque
            {
                ProdutoId = "existing-id",
                Tipo = TipoMovimentoEstoque.Entrada,
                QuantidadeUnidades = 10
            };
            var newMovement = new MovimentacaoEstoque
            {
                ProdutoId = "existing-id",
                Tipo = TipoMovimentoEstoque.Entrada,
                QuantidadeUnidades = 3
            };
            var mockMovCol = MongoDbTestHelpers.CreateMockCollection(new List<MovimentacaoEstoque> { existingMovement, newMovement });

            _mockContext.Produtos.Returns(mockProdutosCol);
            _mockContext.MovimentacoesEstoque.Returns(mockMovCol);

            // Act
            var result = await _sut.AdicionarEstoqueAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.ProdutoId.Should().Be("existing-id");
            result.QuantidadeTotalProdutoAposEntrada.Should().Be(13); // 10 + 3

            // Verify update was called on database
            await mockProdutosCol.Received(1).UpdateOneAsync(
                Arg.Any<FilterDefinition<Produto>>(),
                Arg.Any<UpdateDefinition<Produto>>(),
                Arg.Any<UpdateOptions>(),
                Arg.Any<CancellationToken>()
            );

            // Verify movement was inserted
            await mockMovCol.Received(1).InsertOneAsync(
                Arg.Is<MovimentacaoEstoque>(m => m.ProdutoId == "existing-id" && m.QuantidadeUnidades == 3),
                Arg.Any<InsertOneOptions>(),
                Arg.Any<CancellationToken>()
            );
        }

        [Fact]
        public async Task AdicionarEstoqueAsync_WhenProductExistsByBarcode_UpdatesProductAndInsertsMovement()
        {
            // Arrange
            var existingProduto = new Produto
            {
                Id = "existing-id-by-barcode",
                Nome = "Barcode Name",
                CodigoBarras = "789"
            };

            var request = new AdicionarEstoqueRequestDto
            {
                Produto = new ProdutoUpsertDto
                {
                    CodigoBarras = "789",
                    Nome = "Updated Barcode Name"
                },
                Movimentacao = new MovimentacaoEstoqueCreateDto
                {
                    Tipo = TipoMovimentoEstoque.Entrada,
                    QuantidadeUnidades = 2
                }
            };

            var mockProdutosCol = MongoDbTestHelpers.CreateMockCollection(new List<Produto> { existingProduto });
            var mockMovCol = MongoDbTestHelpers.CreateMockCollection(new List<MovimentacaoEstoque>());

            _mockContext.Produtos.Returns(mockProdutosCol);
            _mockContext.MovimentacoesEstoque.Returns(mockMovCol);

            // Act
            var result = await _sut.AdicionarEstoqueAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.ProdutoId.Should().Be("existing-id-by-barcode");

            // Verify update was called
            await mockProdutosCol.Received(1).UpdateOneAsync(
                Arg.Any<FilterDefinition<Produto>>(),
                Arg.Any<UpdateDefinition<Produto>>(),
                Arg.Any<UpdateOptions>(),
                Arg.Any<CancellationToken>()
            );
        }

        [Fact]
        public async Task RegistrarMovimentacaoAsync_InsertsMovementWithCorrectValues()
        {
            // Arrange
            var dto = new MovimentacaoEstoqueDto
            {
                ProdutoId = "prod-123",
                Tipo = TipoMovimentoEstoque.Saida,
                QuantidadeUnidades = 4,
                DataMovimento = DateTime.UtcNow,
                Motivo = "Consumo"
            };

            var mockMovCol = Substitute.For<IMongoCollection<MovimentacaoEstoque>>();
            _mockContext.MovimentacoesEstoque.Returns(mockMovCol);

            // Act
            await _sut.RegistrarMovimentacaoAsync(dto);

            // Assert
            await mockMovCol.Received(1).InsertOneAsync(
                Arg.Is<MovimentacaoEstoque>(m => 
                    m.ProdutoId == "prod-123" && 
                    m.Tipo == TipoMovimentoEstoque.Saida && 
                    m.QuantidadeUnidades == 4 && 
                    m.Motivo == "Consumo"),
                Arg.Any<InsertOneOptions>(),
                Arg.Any<CancellationToken>()
            );
        }
    }
}
