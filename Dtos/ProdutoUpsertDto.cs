using Grocery.Shopping.API.Enums;

namespace Grocery.Shopping.API.Dtos
{
    public class ProdutoUpsertDto
    {
        public string? Id { get; set; }  // string com ObjectId, já que a entidade usa string

        public string Nome { get; set; } = null!;
        public string? Marca { get; set; }
        public string UnidadeMedida { get; set; } = null!; // kg, g, L, ml, un
        public CategoriaProduto Categoria { get; set; }
        public string? CodigoBarras { get; set; }
        public string? ImagemUrl { get; set; }
    }
}
