using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Grocery.Shopping.API.Application.Interfaces;
using Grocery.Shopping.API.Arguments;
using Grocery.Shopping.API.Controllers;
using Grocery.Shopping.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Grocery.Shopping.API.Tests.Controllers
{
    public class ProdutosControllerTests
    {
        private readonly IReconhecimentoFotoService _mockReconhecimentoFotoService;
        private readonly ProdutosController _sut;

        public ProdutosControllerTests()
        {
            _mockReconhecimentoFotoService = Substitute.For<IReconhecimentoFotoService>();
            _sut = new ProdutosController(_mockReconhecimentoFotoService);
        }

        [Fact]
        public async Task ReconhecerPorFoto_WithNullFoto_ReturnsBadRequest()
        {
            // Arrange
            var request = new ReconhecerFotoRequest
            {
                Foto = null!
            };

            // Act
            var result = await _sut.ReconhecerPorFoto(request, CancellationToken.None);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("O arquivo 'foto' é obrigatório e não pode estar vazio.");
        }

        [Fact]
        public async Task ReconhecerPorFoto_WithEmptyFoto_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(0);
            var request = new ReconhecerFotoRequest
            {
                Foto = mockFile
            };

            // Act
            var result = await _sut.ReconhecerPorFoto(request, CancellationToken.None);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("O arquivo 'foto' é obrigatório e não pode estar vazio.");
        }

        [Fact]
        public async Task ReconhecerPorFoto_WithValidFoto_CallsServiceAndReturnsOk()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(100);
            var request = new ReconhecerFotoRequest
            {
                Foto = mockFile
            };

            var expectedResponse = new ReconhecimentoFotoResponseDto
            {
                ProdutoSugerido = new ProdutoReconhecidoDto
                {
                    NomeProduto = new CampoReconhecidoDto<string> { Valor = "Arroz" }
                }
            };

            _mockReconhecimentoFotoService.ReconhecerProdutoAsync(mockFile, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _sut.ReconhecerPorFoto(request, CancellationToken.None);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().Be(expectedResponse);
        }
    }
}
