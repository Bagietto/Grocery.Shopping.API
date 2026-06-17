using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Grocery.Shopping.API.Application.Interfaces;
using Grocery.Shopping.API.Controllers;
using Grocery.Shopping.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Grocery.Shopping.API.Tests.Controllers
{
    public class EstoqueControllerTests
    {
        private readonly IEstoqueService _mockEstoqueService;
        private readonly EstoqueController _sut;

        public EstoqueControllerTests()
        {
            _mockEstoqueService = Substitute.For<IEstoqueService>();
            _sut = new EstoqueController(_mockEstoqueService);
        }

        [Fact]
        public async Task Adicionar_WithNullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _sut.Adicionar(null!, CancellationToken.None);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("Payload não pode ser nulo.");
        }

        [Fact]
        public async Task Adicionar_WithNullProduto_ReturnsBadRequest()
        {
            // Arrange
            var request = new AdicionarEstoqueRequestDto
            {
                Produto = null!,
                Movimentacao = new MovimentacaoEstoqueCreateDto()
            };

            // Act
            var result = await _sut.Adicionar(request, CancellationToken.None);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("Dados do produto são obrigatórios.");
        }

        [Fact]
        public async Task Adicionar_WithNullMovimentacao_ReturnsBadRequest()
        {
            // Arrange
            var request = new AdicionarEstoqueRequestDto
            {
                Produto = new ProdutoUpsertDto(),
                Movimentacao = null!
            };

            // Act
            var result = await _sut.Adicionar(request, CancellationToken.None);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("Dados da movimentação são obrigatórios.");
        }

        [Fact]
        public async Task Adicionar_WithInvalidQuantity_ReturnsBadRequest()
        {
            // Arrange
            var request = new AdicionarEstoqueRequestDto
            {
                Produto = new ProdutoUpsertDto(),
                Movimentacao = new MovimentacaoEstoqueCreateDto { QuantidadeUnidades = 0 }
            };

            // Act
            var result = await _sut.Adicionar(request, CancellationToken.None);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("Quantidade de unidades deve ser maior que zero.");
        }

        [Fact]
        public async Task Adicionar_WithValidRequest_ReturnsOkWithResponse()
        {
            // Arrange
            var request = new AdicionarEstoqueRequestDto
            {
                Produto = new ProdutoUpsertDto(),
                Movimentacao = new MovimentacaoEstoqueCreateDto { QuantidadeUnidades = 5 }
            };

            var expectedResponse = new AdicionarEstoqueResponseDto
            {
                ProdutoId = "prod-123",
                MovimentacaoId = "mov-123",
                QuantidadeTotalProdutoAposEntrada = 10,
                Mensagem = "Sucesso"
            };

            _mockEstoqueService.AdicionarEstoqueAsync(request, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _sut.Adicionar(request, CancellationToken.None);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task Movimentar_WithNullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _sut.Movimentar(null!, CancellationToken.None);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("Payload não pode ser nulo.");
        }

        [Fact]
        public async Task Movimentar_WithNullOrEmptyProdutoId_ReturnsBadRequest()
        {
            // Arrange
            var request = new MovimentacaoEstoqueDto
            {
                ProdutoId = ""
            };

            // Act
            var result = await _sut.Movimentar(request, CancellationToken.None);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("ProdutoId é obrigatório.");
        }

        [Fact]
        public async Task Movimentar_WithZeroQuantity_ReturnsBadRequest()
        {
            // Arrange
            var request = new MovimentacaoEstoqueDto
            {
                ProdutoId = "prod-123",
                QuantidadeUnidades = 0
            };

            // Act
            var result = await _sut.Movimentar(request, CancellationToken.None);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("Quantidade deve ser maior que zero.");
        }

        [Fact]
        public async Task Movimentar_WithValidRequest_CallsServiceAndReturnsOk()
        {
            // Arrange
            var request = new MovimentacaoEstoqueDto
            {
                ProdutoId = "prod-123",
                QuantidadeUnidades = 5
            };

            // Act
            var result = await _sut.Movimentar(request, CancellationToken.None);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            
            await _mockEstoqueService.Received(1).RegistrarMovimentacaoAsync(request, Arg.Any<CancellationToken>());
        }
    }
}
