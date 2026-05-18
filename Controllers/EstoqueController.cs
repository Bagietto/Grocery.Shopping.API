using Grocery.Shopping.API.Application.Interfaces;
using Grocery.Shopping.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Grocery.Shopping.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstoqueController : ControllerBase
    {
        private readonly IEstoqueService _estoqueService;

        public EstoqueController(IEstoqueService estoqueService)
        {
            _estoqueService = estoqueService;
        }

        /// <summary>
        /// Confirma os dados do produto (novo ou existente) e registra
        /// uma movimentação de ENTRADA no estoque.
        /// </summary>
        /// <remarks>
        /// Esse endpoint é o que você vai chamar após o reconhecimento por IA.
        /// O payload deve conter:
        /// - Produto (ProdutoUpsertDto)
        /// - Movimentacao (MovimentacaoEstoqueCreateDto) com Tipo = Entrada
        /// </remarks>
        [HttpPost("adicionar")]
        [ProducesResponseType(typeof(AdicionarEstoqueResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AdicionarEstoqueResponseDto>> Adicionar(
            [FromBody] AdicionarEstoqueRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null)
                return BadRequest("Payload não pode ser nulo.");

            if (request.Produto is null)
                return BadRequest("Dados do produto são obrigatórios.");

            if (request.Movimentacao is null)
                return BadRequest("Dados da movimentação são obrigatórios.");

            if (request.Movimentacao.QuantidadeUnidades <= 0)
                return BadRequest("Quantidade de unidades deve ser maior que zero.");

            var resposta = await _estoqueService
                .AdicionarEstoqueAsync(request, cancellationToken);

            return Ok(resposta);
        }

        /// <summary>
        /// Registra uma movimentação de estoque (Entrada, Saída ou Ajuste) manualmente.
        /// </summary>
        /// <remarks>
        /// Use esse endpoint, por exemplo, para:
        /// - Baixar consumo (Saída)
        /// - Ajustar estoque por quebra/perda (Ajuste)
        /// - Lançar uma entrada manual (Entrada)
        /// </remarks>
        [HttpPost("movimentar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Movimentar(
            [FromBody] MovimentacaoEstoqueDto movimento,
            CancellationToken cancellationToken)
        {
            if (movimento is null)
                return BadRequest("Payload não pode ser nulo.");

            if (string.IsNullOrWhiteSpace(movimento.ProdutoId))
                return BadRequest("ProdutoId é obrigatório.");

            if (movimento.QuantidadeUnidades <= 0)
                return BadRequest("Quantidade deve ser maior que zero.");

            await _estoqueService.RegistrarMovimentacaoAsync(movimento, cancellationToken);

            return Ok(new
            {
                Mensagem = "Movimentação registrada com sucesso."
            });
        }

        ///// <summary>
        ///// Retorna o resumo do estoque atual: produtos, saldo e próxima data de vencimento.
        ///// </summary>
        //[HttpGet]
        //[ProducesResponseType(typeof(List<EstoqueResumoDto>), StatusCodes.Status200OK)]
        //public async Task<ActionResult<List<EstoqueResumoDto>>> ListarEstoque(
        //    CancellationToken cancellationToken)
        //{
        //    var lista = await _estoqueService.ListarEstoqueAsync(cancellationToken);
        //    return Ok(lista);
        //}
    }
}
       
