using Grocery.Shopping.API.Enums;

namespace Grocery.Shopping.API.Dtos
{
    public class CampoReconhecidoDto<T>
    {
        public T? Valor { get; set; }
        public double Confianca { get; set; }

        public bool TemValorComAltaConfianca(double limiar = 0.8)
            => Valor is not null && Confianca >= limiar;
    }

    public class ProdutoReconhecidoDto
    {
        public CampoReconhecidoDto<string> NomeProduto { get; set; } = new();
        public CampoReconhecidoDto<string> Marca { get; set; } = new();
        public CampoReconhecidoDto<decimal> QuantidadeUnidade { get; set; } = new();
        public CampoReconhecidoDto<string> UnidadeMedida { get; set; } = new();
        public CampoReconhecidoDto<string> CategoriaSugestao { get; set; } = new();
        public CampoReconhecidoDto<string> CodigoBarras { get; set; } = new();
    }

    public class ReconhecimentoFotoResponseDto
    {
        public ProdutoReconhecidoDto ProdutoSugerido { get; set; } = new();
        public bool JaExisteNoCatalogo { get; set; }
        public Guid? ProdutoIdExistente { get; set; }
        public List<string> Mensagens { get; set; } = new();
    }

    public class LoteEstoqueDto
    {
        public int QuantidadeUnidades { get; set; }
        public DateTime? DataVencimento { get; set; }
        public string LocalArmazenamento { get; set; } = "Despensa";
        public string? Observacao { get; set; }
    }    

    public class MovimentacaoEstoqueDto
    {
        public string ProdutoId { get; set; }
        public TipoMovimentoEstoque Tipo { get; set; }
        public int QuantidadeUnidades { get; set; }
        public DateTime DataMovimento { get; set; }
        public string? Motivo { get; set; }
        public DateTime DataVencimento { get;  set; }
    }
}
