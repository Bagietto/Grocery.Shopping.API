using Grocery.Shopping.API.Application.Interfaces;
using Grocery.Shopping.API.Arguments;
using Grocery.Shopping.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Grocery.Shopping.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly IReconhecimentoFotoService _reconhecimentoFotoService;

        public ProdutosController(IReconhecimentoFotoService reconhecimentoFotoService)
        {
            _reconhecimentoFotoService = reconhecimentoFotoService;
        }

        [HttpPost("reconhecer-por-foto")]
        [ProducesResponseType(typeof(ReconhecimentoFotoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReconhecimentoFotoResponseDto>> ReconhecerPorFoto(
        [FromForm] ReconhecerFotoRequest request,
        CancellationToken cancellationToken)
        {
            if (request.Foto == null || request.Foto.Length == 0)
                return BadRequest("O arquivo 'foto' é obrigatório e não pode estar vazio.");

            var resultado = await _reconhecimentoFotoService
                .ReconhecerProdutoAsync(request.Foto, cancellationToken);

            return Ok(resultado);
        }
    }
}
