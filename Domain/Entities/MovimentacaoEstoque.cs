using Grocery.Shopping.API.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Grocery.Shopping.API.Domain.Entities
{
    public class MovimentacaoEstoque
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("produtoId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProdutoId { get; set; } = null!;        

        [BsonElement("tipo")]
        public TipoMovimentoEstoque Tipo { get; set; }

        [BsonElement("quantidadeUnidades")]
        public int QuantidadeUnidades { get; set; }

        [BsonElement("dataMovimento")]
        public DateTime DataMovimento { get; set; }

        [BsonElement("dataVencimento")]
        public DateTime DataVencimento { get; set; }
        
        [BsonElement("motivo")]
        public string? Motivo { get; set; }
    }
}
