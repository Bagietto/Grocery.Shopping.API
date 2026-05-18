using Grocery.Shopping.API.Application.Interfaces;
using Grocery.Shopping.API.Dtos;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Grocery.Shopping.API.Application.Services
{
    public class ReconhecimentoFotoService : IReconhecimentoFotoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey;

        // Endpoint e modelo da OpenAI (ajuste se estiver usando outro)
        private const string OpenAiApiUrl = "https://api.openai.com/v1/chat/completions";
        private const string VisionModel = "gpt-4.1-mini"; // ou o modelo com visão que você tiver habilitado

        public ReconhecimentoFotoService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _openAiApiKey = configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI:ApiKey não configurada no appsettings / secrets.");
        }

        public async Task<ReconhecimentoFotoResponseDto> ReconhecerProdutoAsync(
            IFormFile foto,
            CancellationToken ct = default)
        {
            if (foto is null || foto.Length == 0)
                throw new ArgumentException("Arquivo de foto inválido.", nameof(foto));

            // 1. Converte a imagem para base64
            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await foto.CopyToAsync(ms, ct);
                imageBytes = ms.ToArray();
            }

            var base64Image = Convert.ToBase64String(imageBytes);

            // 2. Monta o payload da requisição para o modelo de visão
            var requestBody = BuildOpenAiVisionRequest(base64Image);

            // 3. Prepara o HttpClient
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _openAiApiKey);

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(OpenAiApiUrl, content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(ct);

                return new ReconhecimentoFotoResponseDto
                {
                    ProdutoSugerido = new ProdutoReconhecidoDto(),
                    JaExisteNoCatalogo = false,
                    ProdutoIdExistente = null,
                    Mensagens = new List<string>
                    {
                        $"Falha ao chamar IA de visão ({response.StatusCode}): {errorText}"
                    }
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync(ct);

            // 4. Extrai o conteúdo de texto do chat completion (choices[0].message.content)
            using var visionResult = JsonDocument.Parse(responseJson);

            var contentElement = visionResult
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content");

            var contentNode = contentElement.GetString();

            if (string.IsNullOrWhiteSpace(contentNode))
            {
                return new ReconhecimentoFotoResponseDto
                {
                    ProdutoSugerido = new ProdutoReconhecidoDto(),
                    Mensagens = new List<string> { "IA não retornou conteúdo legível." }
                };
            }

            // 5. Limpar resposta → extrair apenas o JSON válido
            var jsonLimpo = ExtrairJson(contentNode);

            ReconhecimentoFotoResponseDto? resultado;

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                resultado = JsonSerializer.Deserialize<ReconhecimentoFotoResponseDto>(
                    jsonLimpo,
                    options
                );
            }
            catch (Exception ex)
            {
                // Aqui entra justamente o erro que você viu: caractere inválido no início
                return new ReconhecimentoFotoResponseDto
                {
                    ProdutoSugerido = new ProdutoReconhecidoDto(),
                    Mensagens = new List<string>
                    {
                        "Não foi possível interpretar a resposta da IA. Tente tirar outra foto ou ajustar o enquadramento.",
                        ex.Message,
                        $"Conteúdo bruto retornado pela IA: {contentNode}"
                    }
                };
            }

            if (resultado is null)
            {
                return new ReconhecimentoFotoResponseDto
                {
                    ProdutoSugerido = new ProdutoReconhecidoDto(),
                    Mensagens = new List<string>
                    {
                        "A resposta da IA veio vazia ou em formato inesperado."
                    }
                };
            }

            // 6. (Opcional) aqui você pode tentar casar com catálogo de produtos,
            // por exemplo buscando por Código de Barras, Nome+Marca etc.
            // Por enquanto deixo como false.
            resultado.JaExisteNoCatalogo = false;
            resultado.ProdutoIdExistente = null;

            return resultado;
        }

        /// <summary>
        /// Monta o JSON da requisição para o modelo de visão da OpenAI,
        /// incluindo o prompt e a imagem em base64.
        /// </summary>
        private static string BuildOpenAiVisionRequest(string base64Image)
        {
            var prompt = GetVisionPrompt();

            var payload = new
            {
                model = VisionModel,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "Você é um assistente especializado em reconhecer produtos de mercado a partir de fotos de embalagens."
                    },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "text",
                                text = prompt
                            },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = $"data:image/jpeg;base64,{base64Image}"
                                }
                            }
                        }
                    }
                },
                temperature = 0.1
            };

            return JsonSerializer.Serialize(payload);
        }

        /// <summary>
        /// Prompt específico para forçar a IA a responder em JSON estrito
        /// no formato ReconhecimentoFotoResponseDto.
        /// </summary>
        private static string GetVisionPrompt()
        {
            return @"
Analise a imagem de um produto de mercado (alimento, bebida, limpeza, higiene, etc.).
Extraia as seguintes informações da embalagem, se possível:

- nome do produto
- marca
- quantidade por unidade (valor numérico, ex.: 5, 1, 400)
- unidade de medida da embalagem (kg, g, L, ml, un)
- categoria sugerida (graos, enlatados, laticinios, bebidas, limpeza, higiene, outros)
- código de barras (apenas se for claramente legível)

IMPORTANTE:
1. Se um campo não puder ser identificado com confiança, deixe o valor como null e confiança 0.
2. Sempre devolva um JSON VÁLIDO e ESTRITO, sem comentários, sem texto adicional.
3. O JSON deve ter EXATAMENTE o formato abaixo:

{
  ""produtoSugerido"": {
    ""nomeProduto"": { ""valor"": ""string ou null"", ""confianca"": número entre 0 e 1 },
    ""marca"": { ""valor"": ""string ou null"", ""confianca"": número entre 0 e 1 },
    ""quantidadeUnidade"": { ""valor"": número ou null, ""confianca"": número entre 0 e 1 },
    ""unidadeMedida"": { ""valor"": ""string ou null"", ""confianca"": número entre 0 e 1 },
    ""categoriaSugestao"": { ""valor"": ""string ou null"", ""confianca"": número entre 0 e 1 },
    ""codigoBarras"": { ""valor"": ""string ou null"", ""confianca"": número entre 0 e 1 }
  },
  ""jaExisteNoCatalogo"": false,
  ""produtoIdExistente"": null,
  ""mensagens"": []
}

4. Não retorne nenhum texto fora desse JSON.
";
        }

        /// <summary>
        /// Limpa a resposta da IA removendo ```json, ``` e qualquer texto
        /// fora do primeiro '{' e último '}'.
        /// Resolve casos em que o modelo devolve bloco de código Markdown.
        /// </summary>
        private static string ExtrairJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return content;

            var raw = content.Trim();

            // Caso venha com ```json ... ```
            if (raw.StartsWith("```"))
            {
                // Remove as crases e quebras de linha iniciais/finais
                raw = raw.Trim('`', '\n', '\r', ' ');

                // Se ainda sobrou a palavra json no começo, remove
                if (raw.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                {
                    raw = raw.Substring(4).TrimStart('\n', '\r', ' ', '\t');
                }
            }

            // Fallback: pega do primeiro '{' até o último '}'
            var firstBrace = raw.IndexOf('{');
            var lastBrace = raw.LastIndexOf('}');

            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                return raw.Substring(firstBrace, lastBrace - firstBrace + 1);
            }

            // Se não achar, devolve mesmo assim (o Deserialize vai lançar exceção)
            return raw;
        }
    }
}
