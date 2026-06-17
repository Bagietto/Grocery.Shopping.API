using Grocery.Shopping.API.Application.Interfaces;
using Grocery.Shopping.API.Domain.Entities;
using Grocery.Shopping.API.Dtos;
using Grocery.Shopping.API.Enums;
using Grocery.Shopping.API.Infra.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Grocery.Shopping.API.Domain.Services
{
    public class EstoqueService : IEstoqueService
    {
        private readonly IMongoDbContext _context;

        public EstoqueService(IMongoDbContext context)
        {
            _context = context;
        }

        public async Task<AdicionarEstoqueResponseDto> AdicionarEstoqueAsync(
            AdicionarEstoqueRequestDto request,
            CancellationToken ct = default)
        {
            if (request.Produto is null)
                throw new ArgumentNullException(nameof(request.Produto));

            if (request.Movimentacao is null)
                throw new ArgumentNullException(nameof(request.Movimentacao));

            if (request.Movimentacao.QuantidadeUnidades <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(request.Movimentacao.QuantidadeUnidades));

            // 1. Obter ou criar Produto
            var produto = await ObterOuCriarProdutoAsync(request.Produto, ct);

            // 2. Criar Movimentação de Entrada
            var movimentacao = new MovimentacaoEstoque
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProdutoId = produto.Id,
                Tipo = request.Movimentacao.Tipo, // normalmente Entrada
                QuantidadeUnidades = request.Movimentacao.QuantidadeUnidades,
                DataMovimento = DateTime.UtcNow,
                DataVencimento = request.Movimentacao.DataVencimento,
                Motivo = string.IsNullOrWhiteSpace(request.Movimentacao.Motivo)
                    ? "Entrada após reconhecimento por foto"
                    : request.Movimentacao.Motivo
            };

            await _context.MovimentacoesEstoque.InsertOneAsync(movimentacao, cancellationToken: ct);

            // 3. Calcular quantidade total atual do produto
            var quantidadeTotal = await CalcularSaldoProdutoAsync(produto.Id, ct);

            return new AdicionarEstoqueResponseDto
            {
                ProdutoId = produto.Id,
                MovimentacaoId = movimentacao.Id,
                QuantidadeTotalProdutoAposEntrada = quantidadeTotal,
                Mensagem = "Estoque atualizado com sucesso."
            };
        }

        public async Task RegistrarMovimentacaoAsync(
            MovimentacaoEstoqueDto movimentoDto,
            CancellationToken ct = default)
        {
            // Aqui você reutiliza a mesma ideia de salvar movimentações manuais
            // (consumo, ajuste, etc.)
            var movimentacao = new MovimentacaoEstoque
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProdutoId = movimentoDto.ProdutoId.ToString(),
                Tipo = movimentoDto.Tipo,
                QuantidadeUnidades = movimentoDto.QuantidadeUnidades,
                DataMovimento = movimentoDto.DataMovimento == default
                    ? DateTime.UtcNow
                    : movimentoDto.DataMovimento,
                DataVencimento = movimentoDto.DataVencimento, // se for relevante, senão pode ser DateTime.MinValue
                Motivo = movimentoDto.Motivo
            };

            await _context.MovimentacoesEstoque.InsertOneAsync(movimentacao, cancellationToken: ct);
        }

        // ---------------- Métodos privados ----------------

        private async Task<Produto> ObterOuCriarProdutoAsync(
            ProdutoUpsertDto dto,
            CancellationToken ct)
        {
            Produto? produto = null;

            // 1. Se veio Id -> tenta buscar
            if (!string.IsNullOrWhiteSpace(dto.Id))
            {
                produto = await _context.Produtos
                    .Find(x => x.Id == dto.Id)
                    .FirstOrDefaultAsync(ct);
            }

            // 2. Se não achou por Id e tem código de barras, tenta por código de barras
            if (produto is null && !string.IsNullOrWhiteSpace(dto.CodigoBarras))
            {
                produto = await _context.Produtos
                    .Find(x => x.CodigoBarras == dto.CodigoBarras)
                    .FirstOrDefaultAsync(ct);
            }

            // 3. Se ainda não existe, cria novo
            if (produto is null)
            {
                produto = new Produto
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Nome = dto.Nome,
                    Marca = dto.Marca,
                    UnidadeMedida = dto.UnidadeMedida,
                    Categoria = dto.Categoria,
                    CodigoBarras = dto.CodigoBarras,
                    ImagemUrl = dto.ImagemUrl,
                    CriadoEm = DateTime.UtcNow,
                    AtualizadoEm = DateTime.UtcNow
                };

                await _context.Produtos.InsertOneAsync(produto, cancellationToken: ct);
            }
            else
            {
                // 4. Atualiza campos importantes (permitindo correção)
                var update = Builders<Produto>.Update
                    .Set(x => x.Nome, dto.Nome)
                    .Set(x => x.Marca, dto.Marca)
                    .Set(x => x.UnidadeMedida, dto.UnidadeMedida)
                    .Set(x => x.Categoria, dto.Categoria)
                    .Set(x => x.CodigoBarras, dto.CodigoBarras)
                    .Set(x => x.ImagemUrl, dto.ImagemUrl)
                    .Set(x => x.AtualizadoEm, DateTime.UtcNow);

                await _context.Produtos.UpdateOneAsync(
                    x => x.Id == produto.Id,
                    update,
                    cancellationToken: ct);

                produto.Nome = dto.Nome;
                produto.Marca = dto.Marca;
                produto.UnidadeMedida = dto.UnidadeMedida;
                produto.Categoria = dto.Categoria;
                produto.CodigoBarras = dto.CodigoBarras;
                produto.ImagemUrl = dto.ImagemUrl;
                produto.AtualizadoEm = DateTime.UtcNow;
            }

            return produto;
        }

        /// <summary>
        /// Calcula o saldo atual do produto somando todas as movimentações:
        /// Entrada (+), Saída (-), Ajuste (+/- dependendo da sua convenção).
        /// </summary>
        private async Task<int> CalcularSaldoProdutoAsync(string produtoId, CancellationToken ct)
        {
            var filtro = Builders<MovimentacaoEstoque>.Filter.Eq(x => x.ProdutoId, produtoId);

            var movimentacoes = await _context.MovimentacoesEstoque
                .Find(filtro)
                .ToListAsync(ct);

            var saldo = 0;

            foreach (var m in movimentacoes)
            {
                var fator = m.Tipo switch
                {
                    TipoMovimentoEstoque.Entrada => 1,
                    TipoMovimentoEstoque.Saida => -1,
                    TipoMovimentoEstoque.Ajuste => 1, // ou customizado, depende da sua regra
                    _ => 0
                };

                saldo += fator * m.QuantidadeUnidades;
            }

            return saldo;
        }
    }
}