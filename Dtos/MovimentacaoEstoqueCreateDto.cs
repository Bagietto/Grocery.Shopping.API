using Grocery.Shopping.API.Enums;

namespace Grocery.Shopping.API.Dtos
{
    public class MovimentacaoEstoqueCreateDto
    {
        /// <summary>
        /// Tipo da movimentação. Para entrada após reconhecimento, será Entrada.
        /// </summary>
        public TipoMovimentoEstoque Tipo { get; set; } = TipoMovimentoEstoque.Entrada;

        /// <summary>
        /// Quantidade de unidades físicas (ex.: 3 pacotes).
        /// </summary>
        public int QuantidadeUnidades { get; set; }

        /// <summary>
        /// Data de vencimento deste lote/movimentação.
        /// </summary>
        public DateTime DataVencimento { get; set; }

        /// <summary>
        /// Motivo (ex.: ""Entrada após cadastro por foto"").
        /// </summary>
        public string? Motivo { get; set; }
    }
}
