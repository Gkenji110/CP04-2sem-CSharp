# Sistema de Biblioteca — ASP.NET MVC + Oracle (CP04)

Evolução do sistema de gerenciamento de biblioteca (relação N:N entre
**Autores** e **Livros**, persistida em Oracle) construído no CP03. O CP04
adiciona diagnósticos de saúde (Health Checks), logging estruturado com
rastreabilidade (Serilog + Correlation ID), observabilidade corporativa
(OpenTelemetry com Tracing e Métricas) e testes unitários automatizados
(xUnit, Moq e FluentAssertions).

## Sumário

- [Vídeo de demonstração](#vídeo-de-demonstração)
- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Configuração](#configuração)
- [Como rodar](#como-rodar)
- [Endpoints principais](#endpoints-principais)
- [Health Checks](#health-checks)
- [Logging estruturado e Correlation ID](#logging-estruturado-e-correlation-id)
- [Observabilidade com OpenTelemetry](#observabilidade-com-opentelemetry)
- [Testes unitários](#testes-unitários)
- [Estrutura de pastas](#estrutura-de-pastas)

## Vídeo de demonstração

https://youtu.be/0D8JwqmBoJg

## Arquitetura

O projeto principal (`ProjetoBiblioteca`) é uma aplicação ASP.NET MVC
organizada em camadas:

- **Dominio** — contratos de acesso a dados (`IAutorRepositorio`,
  `ILivroRepositorio`), independentes de infraestrutura.
- **Aplicacao** — regras de negócio (`AutorServico`, `LivroServico`) e
  componentes transversais (`CorrelationIdMiddleware`).
- **Infraestrutura** — implementações concretas: acesso a dados via EF Core
  (`Repositorios`), verificação de saúde do banco (`Health`) e observabilidade
  (`Observabilidade`).
- **Controllers / Views / Models** — camada MVC tradicional.

Essa separação existe principalmente para permitir testar as regras de
negócio isoladamente, sem depender do Oracle ou do ASP.NET (ver
[Testes unitários](#testes-unitários)).

Um segundo projeto, `ProjetoBiblioteca.Tests.Unit`, contém a suíte de testes
automatizados e referencia o projeto principal.

## Pré-requisitos

- .NET SDK 8.0
- Acesso ao Oracle da FIAP (usuário/RM e senha do laboratório)
- `dotnet-ef` instalado globalmente, para rodar as migrations:
  ```
  dotnet tool install --global dotnet-ef
  ```

## Configuração

A connection string do Oracle **não** fica no `appsettings.json` (esse
arquivo é versionado no Git e fica público no repositório). Em vez disso,
crie um arquivo `ProjetoBiblioteca/appsettings.Development.json` (ele está no
`.gitignore` e nunca é commitado) com o seguinte conteúdo, substituindo pelo
seu usuário e senha:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=SEU_RM;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/orcl;"
  }
}
```

O ASP.NET Core carrega esse arquivo automaticamente por cima do
`appsettings.json` quando a aplicação roda em ambiente `Development` (é o
que o `Properties/launchSettings.json` já configura por padrão ao usar
`dotnet run`).

## Como rodar

1. Restaure os pacotes:
   ```
   dotnet restore
   ```
2. Configure o `appsettings.Development.json` (ver seção acima).
3. Aplique as migrations no banco Oracle:
   ```
   cd ProjetoBiblioteca
   dotnet ef database update
   ```
4. Rode a aplicação:
   ```
   dotnet run
   ```
5. Acesse `http://localhost:5000` (ou a porta exibida no terminal).

## Endpoints principais

| Rota | Descrição |
|---|---|
| `/` ou `/Livros` | Listagem de livros, com autores associados |
| `/Livros/Create` | Cadastro de livro |
| `/Autores` | Listagem de autores, com livros associados |
| `/Autores/Create` | Cadastro de autor |
| `/health` | Diagnóstico de saúde da aplicação |

## Health Checks

`Infraestrutura/Health/BancoDadosHealthCheck.cs` implementa `IHealthCheck` e
valida a conexão real com o Oracle via
`AppDbContext.Database.CanConnectAsync()`. O resultado fica disponível no
endpoint nativo `/health`, que responde `Healthy` ou `Unhealthy`.

## Logging estruturado e Correlation ID

O logging é feito com **Serilog**, configurado em `Program.cs`:

- Saída em **Console** e em **arquivo rotativo diário**, em
  `ProjetoBiblioteca/logs/app-AAAAMMDD.log`.
- `CorrelationIdMiddleware` (em `Aplicacao/Middlewares`) lê o cabeçalho
  `X-Correlation-ID` da requisição, ou gera um novo GUID caso não exista, e
  injeta esse valor no contexto de log do Serilog. Isso permite rastrear
  todas as linhas de log geradas durante o processamento de uma mesma
  requisição — inclusive a linha automática de resumo gerada por
  `UseSerilogRequestLogging()`.
- Os Controllers têm `ILogger` injetado e registram logs de sucesso, aviso
  (dados inválidos) e erro (exceções de validação).

Exemplo de linha de log, com o Correlation ID entre colchetes:
```
2026-09-08 00:20:20.155 -03:00 [INF] [8beb715d-1905-4963-a860-d66e6e28b7af] HTTP GET / responded 200 in 11.1981 ms
```

## Observabilidade com OpenTelemetry

`Infraestrutura/Observabilidade/AplicacaoMetricas.cs` centraliza:

- Um `Meter` customizado, com dois `Counter<long>`:
  `autores_criados_total` e `livros_criados_total`.
- Um `ActivitySource` customizado, usado para abrir Spans manuais nos
  endpoints de criação de Autor e Livro.

O OpenTelemetry é configurado em `Program.cs` com:

- **Tracing**: auto-instrumentação de ASP.NET Core e HttpClient, mais a
  fonte de Spans customizada, exportando para o Console.
- **Métricas**: auto-instrumentação padrão do ASP.NET Core, mais o `Meter`
  customizado, exportando para o Console.

Ao cadastrar um Autor ou Livro, o console exibe o Span da operação (com tags
como `livro.titulo` e `livro.id`) e, periodicamente, o snapshot das
métricas — incluindo o contador customizado incrementado.

## Testes unitários

O projeto `ProjetoBiblioteca.Tests.Unit` usa **xUnit**, **Moq** e
**FluentAssertions**, seguindo o padrão Arrange-Act-Assert (AAA):

- `Dominio/AutorTests.cs` e `Dominio/LivroTests.cs` — validam a criação com
  dados válidos e o lançamento de `ArgumentException` para dados inválidos
  (nome/título vazio, ano de publicação inválido).
- `Aplicacao/AutorServicoTests.cs` e `Aplicacao/LivroServicoTests.cs` — usam
  `Mock<IAutorRepositorio>` / `Mock<ILivroRepositorio>` para simular o
  repositório e verificam as chamadas com `Verify(..., Times.Once)`.

Para rodar os testes:
```
dotnet test ProjetoBiblioteca.Tests.Unit/ProjetoBiblioteca.Tests.Unit.csproj
```

## Estrutura de pastas

```
ProjetoBiblioteca/
  Aplicacao/
    Middlewares/CorrelationIdMiddleware.cs
    Servicos/AutorServico.cs, IAutorServico.cs, LivroServico.cs, ILivroServico.cs
  Controllers/
    AutoresController.cs
    LivrosController.cs
    HomeController.cs
  Dados/AppDbContext.cs
  Dominio/
    Interfaces/IAutorRepositorio.cs, ILivroRepositorio.cs
  Infraestrutura/
    Health/BancoDadosHealthCheck.cs
    Observabilidade/AplicacaoMetricas.cs
    Repositorios/AutorRepositorio.cs, LivroRepositorio.cs
  Models/
    Autor.cs, Livro.cs, AutorLivro.cs
  Views/
    Autores/, Livros/, Home/, Shared/
  logs/
    app-AAAAMMDD.log (gerado em tempo de execução)
  Program.cs
  appsettings.json
  appsettings.Development.json (não versionado — ver Configuração)

ProjetoBiblioteca.Tests.Unit/
  Dominio/AutorTests.cs, LivroTests.cs
  Aplicacao/AutorServicoTests.cs, LivroServicoTests.cs
```
