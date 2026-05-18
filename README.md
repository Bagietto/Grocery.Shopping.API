# Grocery.Shopping.API

API ASP.NET Core para controle de despensa/estoque de mercado, com cadastro de produtos, movimentacoes de entrada/saida/ajuste e reconhecimento de produto por foto usando IA.

## Visao Geral

O projeto permite:

- reconhecer dados de um produto a partir de uma foto da embalagem;
- sugerir nome, marca, quantidade, unidade de medida, categoria e codigo de barras;
- confirmar o produto reconhecido e registrar entrada no estoque;
- registrar movimentacoes manuais de estoque;
- armazenar produtos e movimentacoes no MongoDB;
- consultar e testar a API via Swagger em ambiente de desenvolvimento.

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- MongoDB
- MongoDB.Driver
- Swagger / Swashbuckle
- OpenAI Vision via HTTP

## Estrutura do Projeto

```text
Grocery.Shopping.API/
|-- Application/
|   |-- Interfaces/
|   `-- Services/
|-- Arguments/
|-- Controllers/
|-- Domain/
|   |-- Entities/
|   |-- Interfaces/
|   `-- Services/
|-- Dtos/
|-- Enums/
|-- Infra/
|   `-- Mongo/
|-- Properties/
|-- Program.cs
|-- appsettings.json
`-- Grocery.Shopping.API.csproj
```

Principais responsabilidades:

- `Controllers`: expoem os endpoints HTTP.
- `Application/Services`: integracoes de aplicacao, como reconhecimento por foto.
- `Domain/Services`: regras de estoque e persistencia de movimentacoes.
- `Infra/Mongo`: configuracao e acesso as collections do MongoDB.
- `Dtos`: contratos de entrada e saida da API.
- `Enums`: categorias de produto e tipos de movimentacao.

## Funcionalidades

### Reconhecimento de Produto por Foto

Endpoint:

```http
POST /Produtos/reconhecer-por-foto
Content-Type: multipart/form-data
```

Campo esperado:

- `foto`: arquivo de imagem do produto.

A API envia a imagem para o modelo configurado no servico `ReconhecimentoFotoService` e espera uma resposta JSON com os campos reconhecidos e seus niveis de confianca.

Exemplo de resposta:

```json
{
  "produtoSugerido": {
    "nomeProduto": {
      "valor": "Arroz Tipo 1",
      "confianca": 0.92
    },
    "marca": {
      "valor": "Marca Exemplo",
      "confianca": 0.86
    },
    "quantidadeUnidade": {
      "valor": 5,
      "confianca": 0.95
    },
    "unidadeMedida": {
      "valor": "kg",
      "confianca": 0.95
    },
    "categoriaSugestao": {
      "valor": "Graos",
      "confianca": 0.8
    },
    "codigoBarras": {
      "valor": null,
      "confianca": 0
    }
  },
  "jaExisteNoCatalogo": false,
  "produtoIdExistente": null,
  "mensagens": []
}
```

### Adicionar Produto ao Estoque

Endpoint:

```http
POST /api/Estoque/adicionar
Content-Type: application/json
```

Cria ou atualiza um produto e registra uma movimentacao de estoque, normalmente do tipo `Entrada`.

Exemplo:

```json
{
  "produto": {
    "nome": "Arroz Tipo 1",
    "marca": "Marca Exemplo",
    "unidadeMedida": "kg",
    "categoria": 1,
    "codigoBarras": "7890000000000",
    "imagemUrl": null
  },
  "movimentacao": {
    "tipo": 1,
    "quantidadeUnidades": 3,
    "dataVencimento": "2026-12-31T00:00:00Z",
    "motivo": "Entrada apos reconhecimento por foto"
  }
}
```

Exemplo de resposta:

```json
{
  "produtoId": "665000000000000000000000",
  "movimentacaoId": "665000000000000000000001",
  "quantidadeTotalProdutoAposEntrada": 3,
  "mensagem": "Estoque atualizado com sucesso."
}
```

### Registrar Movimentacao Manual

Endpoint:

```http
POST /api/Estoque/movimentar
Content-Type: application/json
```

Registra uma entrada, saida ou ajuste de estoque para um produto existente.

Exemplo:

```json
{
  "produtoId": "665000000000000000000000",
  "tipo": 2,
  "quantidadeUnidades": 1,
  "dataMovimento": "2026-05-18T12:00:00Z",
  "dataVencimento": "2026-12-31T00:00:00Z",
  "motivo": "Consumo"
}
```

Resposta:

```json
{
  "mensagem": "Movimentacao registrada com sucesso."
}
```

## Enums Importantes

### TipoMovimentoEstoque

| Valor | Nome | Descricao |
| --- | --- | --- |
| 1 | Entrada | Adiciona unidades ao estoque |
| 2 | Saida | Remove unidades do estoque |
| 3 | Ajuste | Registra correcao manual |

### CategoriaProduto

O projeto possui uma lista ampla de categorias, incluindo:

- `Graos`
- `Massas`
- `Enlatados`
- `Laticinios`
- `BebidasNaoAlcoolicas`
- `Higiene`
- `Limpeza`
- `PetRacoes`
- `Outros`

Consulte `Enums/CategoriaProduto.cs` para a lista completa e seus valores numericos.

## Configuracao

O arquivo `appsettings.json` deve conter apenas configuracoes compartilhaveis. Valores sensiveis, como chave da OpenAI e connection string real, devem ser definidos por variaveis de ambiente, User Secrets ou um `appsettings.Development.json` local ignorado pelo Git.

Exemplo de configuracao local:

```json
{
  "OpenAI": {
    "ApiKey": "sua-chave-openai"
  },
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "DespensaDb"
  }
}
```

### User Secrets

Opcionalmente, configure os segredos com:

```powershell
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sua-chave-openai"
dotnet user-secrets set "Mongo:ConnectionString" "mongodb://localhost:27017"
dotnet user-secrets set "Mongo:DatabaseName" "DespensaDb"
```

## Como Executar Localmente

### Pre-requisitos

- .NET SDK 8
- MongoDB local ou remoto
- Chave de API da OpenAI para o reconhecimento por foto

### Restaurar dependencias

```powershell
dotnet restore
```

### Executar a API

```powershell
dotnet run
```

Perfis locais configurados:

- HTTP: `http://localhost:5202`
- HTTPS: `https://localhost:7041`

Em ambiente `Development`, a documentacao Swagger fica disponivel em:

```text
http://localhost:5202/swagger
https://localhost:7041/swagger
```

## Collections MongoDB

A API utiliza as seguintes collections:

- `produtos`
- `movimentacoesEstoque`

Modelo simplificado:

- `Produto`: nome, marca, unidade de medida, categoria, codigo de barras, imagem e datas de auditoria.
- `MovimentacaoEstoque`: produto, tipo, quantidade, data do movimento, data de vencimento e motivo.

## Regras Atuais de Estoque

- Produto pode ser localizado por `Id`.
- Se nao houver `Id`, a API tenta localizar por `CodigoBarras`.
- Se o produto nao existir, ele e criado.
- Se o produto existir, seus dados principais sao atualizados.
- O saldo atual e calculado somando movimentacoes:
  - `Entrada`: soma quantidade.
  - `Saida`: subtrai quantidade.
  - `Ajuste`: atualmente soma quantidade.

## Observacoes de Seguranca

- Nao versionar `appsettings.Development.json`.
- Nao versionar chaves de API.
- Rotacionar qualquer chave exposta acidentalmente.
- Preferir User Secrets em desenvolvimento.
- Preferir variaveis de ambiente ou gerenciador de segredos em producao.

## Status do Projeto

Implementado:

- API ASP.NET Core .NET 8.
- Swagger em ambiente de desenvolvimento.
- Integracao com MongoDB.
- Reconhecimento de produto por foto.
- Cadastro/atualizacao de produto.
- Movimentacoes de estoque.

Pontos naturais para evolucao:

- endpoint de listagem/resumo de estoque;
- testes automatizados;
- validacoes com Data Annotations ou FluentValidation;
- autenticacao/autorizacao;
- indices MongoDB para `CodigoBarras` e `ProdutoId`;
- tratamento padronizado de erros;
- migracao da chamada OpenAI para SDK oficial ou Responses API.
