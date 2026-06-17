using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Grocery.Shopping.API.Application.Services;
using Grocery.Shopping.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Grocery.Shopping.API.Tests.Services
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _sendAsync(request, cancellationToken);
        }
    }

    public class ReconhecimentoFotoServiceTests
    {
        private readonly IConfiguration _mockConfiguration;

        public ReconhecimentoFotoServiceTests()
        {
            _mockConfiguration = Substitute.For<IConfiguration>();
            _mockConfiguration["OpenAI:ApiKey"].Returns("test-api-key");
        }

        [Fact]
        public async Task ReconhecerProdutoAsync_WithNullFoto_ThrowsArgumentException()
        {
            // Arrange
            var service = new ReconhecimentoFotoService(new HttpClient(), _mockConfiguration);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.ReconhecerProdutoAsync(null!));
        }

        [Fact]
        public async Task ReconhecerProdutoAsync_WithEmptyFoto_ThrowsArgumentException()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(0);
            var service = new ReconhecimentoFotoService(new HttpClient(), _mockConfiguration);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.ReconhecerProdutoAsync(mockFile));
        }

        [Fact]
        public async Task ReconhecerProdutoAsync_WhenOpenAiReturnsSuccess_ReturnsStructuredResponse()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(10);
            mockFile.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    var stream = x.Arg<Stream>();
                    var bytes = new byte[] { 1, 2, 3, 4 };
                    stream.Write(bytes, 0, bytes.Length);
                    return Task.CompletedTask;
                });

            var openAiContent = @"
            {
              ""produtoSugerido"": {
                ""nomeProduto"": { ""valor"": ""Feijao Preto"", ""confianca"": 0.95 },
                ""marca"": { ""valor"": ""Kicaldo"", ""confianca"": 0.98 }
              },
              ""jaExisteNoCatalogo"": false,
              ""produtoIdExistente"": null,
              ""mensagens"": []
            }";

            var openAiResponse = new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = "```json\n" + openAiContent + "\n```"
                        }
                    }
                }
            };

            var fakeHandler = new FakeHttpMessageHandler((req, ct) =>
            {
                req.Headers.Authorization.Should().NotBeNull();
                req.Headers.Authorization!.Parameter.Should().Be("test-api-key");

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(openAiResponse), Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            });

            var httpClient = new HttpClient(fakeHandler);
            var service = new ReconhecimentoFotoService(httpClient, _mockConfiguration);

            // Act
            var result = await service.ReconhecerProdutoAsync(mockFile);

            // Assert
            result.Should().NotBeNull();
            result.ProdutoSugerido.Should().NotBeNull();
            result.ProdutoSugerido.NomeProduto.Valor.Should().Be("Feijao Preto");
            result.ProdutoSugerido.Marca.Valor.Should().Be("Kicaldo");
            result.Mensagens.Should().BeEmpty();
        }

        [Fact]
        public async Task ReconhecerProdutoAsync_WhenOpenAiReturnsHttpError_ReturnsResponseWithErrorMessage()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(10);
            mockFile.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    var stream = x.Arg<Stream>();
                    var bytes = new byte[] { 1, 2, 3, 4 };
                    stream.Write(bytes, 0, bytes.Length);
                    return Task.CompletedTask;
                });

            var fakeHandler = new FakeHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Bad Request Error Details")
                };
                return Task.FromResult(response);
            });

            var httpClient = new HttpClient(fakeHandler);
            var service = new ReconhecimentoFotoService(httpClient, _mockConfiguration);

            // Act
            var result = await service.ReconhecerProdutoAsync(mockFile);

            // Assert
            result.Should().NotBeNull();
            result.Mensagens.Should().ContainSingle(m => m.Contains("Falha ao chamar IA de visão (BadRequest)"));
        }

        [Fact]
        public async Task ReconhecerProdutoAsync_WhenOpenAiReturnsInvalidJson_ReturnsErrorDetailsInMessages()
        {
            // Arrange
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(10);
            mockFile.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    var stream = x.Arg<Stream>();
                    stream.Write(new byte[] { 1 }, 0, 1);
                    return Task.CompletedTask;
                });

            var openAiResponse = new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = "invalid-json-content"
                        }
                    }
                }
            };

            var fakeHandler = new FakeHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(openAiResponse), Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            });

            var httpClient = new HttpClient(fakeHandler);
            var service = new ReconhecimentoFotoService(httpClient, _mockConfiguration);

            // Act
            var result = await service.ReconhecerProdutoAsync(mockFile);

            // Assert
            result.Should().NotBeNull();
            result.Mensagens.Should().Contain(m => m.Contains("Não foi possível interpretar a resposta da IA."));
        }
    }
}
