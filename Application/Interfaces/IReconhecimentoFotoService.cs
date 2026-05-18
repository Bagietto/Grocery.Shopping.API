using Grocery.Shopping.API.Dtos;

namespace Grocery.Shopping.API.Application.Interfaces
{
    public interface IReconhecimentoFotoService
    {
        /// <summary>
        /// Processa a imagem do produto, chama a IA e retorna os dados sugeridos.
        /// </summary>
        Task<ReconhecimentoFotoResponseDto> ReconhecerProdutoAsync(IFormFile foto, CancellationToken ct = default);
    }

    public interface IEstoqueService
    {
        /// <summary>
        /// Cria/atualiza o produto e registra a entrada do lote em estoque.
        /// </summary>
        Task<AdicionarEstoqueResponseDto> AdicionarEstoqueAsync(AdicionarEstoqueRequestDto request, CancellationToken ct = default);

        /// <summary>
        /// Registra movimentações de estoque (consumo, ajuste, etc.).
        /// </summary>
        Task RegistrarMovimentacaoAsync(MovimentacaoEstoqueDto movimento, CancellationToken ct = default);
    }

}
