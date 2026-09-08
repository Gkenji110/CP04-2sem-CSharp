# ProjetoBiblioteca.Tests.Unit

Suíte de testes unitários do projeto `ProjetoBiblioteca`, focada em validar as
regras de negócio da camada `Aplicacao` (Serviços) de forma isolada, sem
depender do Oracle nem do ASP.NET Core.

## Relação com o projeto principal

Este projeto referencia diretamente o `ProjetoBiblioteca` (via
`ProjectReference` no `.csproj`) e testa suas classes de domínio e serviço:

- `AutorServico` / `ILivroServico` — regras de criação, busca e listagem.
- Validações de negócio: nome/título obrigatório, ano de publicação válido.

A camada de acesso a dados (`IAutorRepositorio` / `ILivroRepositorio`) é
simulada com **mocks**, então os testes não precisam de conexão real com o
banco — é justamente essa separação em interfaces (`Dominio/Interfaces`) que
existe no projeto principal que torna esses testes possíveis.

## Ferramentas utilizadas

- **xUnit** — framework de testes (`[Fact]`, `[Theory]`, `[InlineData]`).
- **Moq** — criação de mocks para `IAutorRepositorio` e `ILivroRepositorio`.
- **FluentAssertions** — assertivas mais legíveis (`resultado.Should().Be(...)`).
- Padrão **AAA** (Arrange, Act, Assert) em todos os testes.

## Estrutura

```
ProjetoBiblioteca.Tests.Unit/
  Dominio/
    AutorTests.cs      Cria Autor com dados válidos e inválidos (via AutorServico)
    LivroTests.cs       Cria Livro com dados válidos e inválidos (via LivroServico)
  Aplicacao/
    AutorServicoTests.cs   Testa BuscarPorId, ListarTodos e Criar de AutorServico
    LivroServicoTests.cs    Testa BuscarPorId, ListarTodos e Criar de LivroServico
```

## O que é validado

- Criação bem-sucedida chama o repositório (`Adicionar`) exatamente uma vez
  (`Times.Once`).
- Dados inválidos (nome/título vazio, ano de publicação <= 0) lançam
  `ArgumentException` e **não** chamam o repositório (`Times.Never`).
- `BuscarPorId` e `ListarTodos` retornam os dados simulados pelo mock
  corretamente.

## Como rodar

A partir da raiz do repositório:

```
dotnet test ProjetoBiblioteca.Tests.Unit/ProjetoBiblioteca.Tests.Unit.csproj
```

Ou entrando na pasta do projeto:

```
cd ProjetoBiblioteca.Tests.Unit
dotnet test
```

Resultado esperado: `Passed! - Failed: 0, Passed: 17, Skipped: 0, Total: 17`.

Para ver o nome de cada teste no output:

```
dotnet test ProjetoBiblioteca.Tests.Unit/ProjetoBiblioteca.Tests.Unit.csproj -v normal
```
