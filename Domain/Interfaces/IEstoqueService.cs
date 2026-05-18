using Grocery.Shopping.API.Dtos;

namespace Grocery.Shopping.API.Domain.Interfaces
{
    public interface IEstoqueService
    {
        Task<AdicionarEstoqueResponseDto> AdicionarEstoqueAsync(
            AdicionarEstoqueRequestDto request,
            CancellationToken ct = default);

        Task RegistrarMovimentacaoAsync(
            MovimentacaoEstoqueDto movimento,  // você pode manter esse DTO p/ API genérica de movimentação
            CancellationToken ct = default);
    }

}
