namespace Grocery.Shopping.API.Dtos
{
    public class AdicionarEstoqueResponseDto
    {
        public string ProdutoId { get; set; } = null!;
        public string MovimentacaoId { get; set; } = null!;
        public int QuantidadeTotalProdutoAposEntrada { get; set; }

        public string Mensagem { get; set; } = "Estoque atualizado com sucesso.";
    }

}
