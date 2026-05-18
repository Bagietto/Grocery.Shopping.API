using Grocery.Shopping.API.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Grocery.Shopping.API.Domain.Entities
{
    public class Produto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("nome")]
        public string Nome { get; set; } = null!;

        [BsonElement("marca")]
        public string? Marca { get; set; }       

        [BsonElement("unidadeMedida")]
        public string UnidadeMedida { get; set; } = null!; // kg, g, L, ml, un

        [BsonRepresentation(BsonType.String)]
        public CategoriaProduto Categoria { get; set; }

        [BsonElement("codigoBarras")]
        public string? CodigoBarras { get; set; }

        [BsonElement("imagemUrl")]
        public string? ImagemUrl { get; set; }

        [BsonElement("criadoEm")]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        [BsonElement("atualizadoEm")]
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    }
}
