# 📝 Sugestão de Post para o LinkedIn

Aqui está um modelo de post otimizado para gerar engajamento, destacar suas competências técnicas e apresentar o projeto de forma profissional.

---

🚀 **Automatizando a gestão de estoque com Inteligência Artificial e .NET 8!**

Registrar produtos e fazer "data entry" manual é um processo lento e sujeito a erros. E sejamos francos: OCRs tradicionais costumam falhar quando encontram embalagens coloridas, fontes estilizadas ou designs modernos.

Para resolver isso de forma inteligente, desenvolvi o **Grocery.Shopping.API** — uma Web API em **ASP.NET Core (.NET 8)** que integra um **Agente de Reconhecimento Visual** usando IA para automatizar fluxos de entrada de dados de produtos.

### 🧠 Como funciona por trás dos panos?
1. **Entrada de Imagem:** A API recebe a foto da embalagem de um produto.
2. **Agente de Visão (OpenAI):** A imagem é processada e enviada ao modelo `gpt-4o-mini` com instruções estritas de prompt e formatação.
3. **Estruturação de Dados:** Em vez de texto bruto, a IA extrai semanticamente campos como *Nome do Produto, Marca, Quantidade, Unidade de Medida* e *Categoria*, acompanhados de um índice de confiança (de 0 a 1) para cada informação.
4. **Persistência Sem Complicações:** O JSON estruturado é validado pelo backend e persistido de forma flexível em um banco NoSQL utilizando **MongoDB**.

### 🛠️ Diferenciais Técnicos do Projeto:
* **LLMs em Fluxo de Trabalho Backend:** Integração nativa de modelos multimodais de visão em arquitetura .NET.
* **Resiliência na Desserialização:** Tratamento avançado de strings para extração e limpeza do JSON retornado pela IA, evitando falhas de parser.
* **Clean Architecture Lite:** Separação clara de responsabilidades entre regras de domínio (`EstoqueService`), integrações externas (`ReconhecimentoFotoService`) e endpoints HTTP RESTful.
* **Segurança:** Configuração limpa via *User Secrets* para blindar chaves de API em ambientes públicos de desenvolvimento.

O código-fonte está 100% aberto e documentado com Swagger e diagramas de arquitetura no meu GitHub! 

Se você trabalha com .NET, IA ou arquitetura de software, adoraria receber seu feedback e ideias de evolução! Qual outro caso de uso de Vision APIs você acha interessante para o dia a dia?

🔗 **Link do Repositório:** [Insira aqui o link do seu repositório no GitHub]

#dotnet #csharp #backend #openai #artificialintelligence #mongodb #nosql #github #opensource #softwarearchitecture #webapi #agenteia
