using Grocery.Shopping.API.Domain.Entities;
using MongoDB.Driver;

namespace Grocery.Shopping.API.Infra.Mongo
{
    public interface IMongoDbContext
    {
        IMongoCollection<Produto> Produtos { get; }
        IMongoCollection<MovimentacaoEstoque> MovimentacoesEstoque { get; }
    }
}
