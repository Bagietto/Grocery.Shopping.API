namespace Grocery.Shopping.API.Dtos
{
    public class AdicionarEstoqueRequestDto
    {
        public ProdutoUpsertDto Produto { get; set; } = new();
        public MovimentacaoEstoqueCreateDto Movimentacao { get; set; } = new();
    }
}
