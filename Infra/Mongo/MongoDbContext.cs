using Grocery.Shopping.API.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Grocery.Shopping.API.Infra.Mongo
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Produto> Produtos
            => _database.GetCollection<Produto>("produtos");

        public IMongoCollection<MovimentacaoEstoque> MovimentacoesEstoque
            => _database.GetCollection<MovimentacaoEstoque>("movimentacoesEstoque");
    }
}
