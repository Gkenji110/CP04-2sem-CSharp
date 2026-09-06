# Sistema de Biblioteca — ASP.NET MVC + Oracle (Relação N:N)

Projeto gerado seguindo o mesmo padrão da aula "Relação N:N com ASP.NET MVC e
Oracle" (Jogos/Plataformas), adaptado para **Autores** e **Livros**.

## Estrutura

- `Models/Autor.cs` — entidade Autor
- `Models/Livro.cs` — entidade Livro
- `Models/AutorLivro.cs` — tabela intermediária (chave composta AutorId + LivroId)
- `Dados/AppDbContext.cs` — contexto EF Core, com `OnModelCreating` configurando
  a chave composta e os dois lados do relacionamento N:N
- `Controllers/AutoresController.cs` — CRUD completo de Autores, com
  associação/desassociação de Livros
- `Controllers/LivrosController.cs` — CRUD completo de Livros, com
  associação/desassociação de Autores
- `Views/Autores/*` e `Views/Livros/*` — telas de Index, Create, Edit, Details
  e Delete

## Como rodar

1. Instale o SDK do .NET 8.
2. Abra a pasta do projeto e restaure os pacotes:
   ```
   dotnet restore
   ```
3. Edite `appsettings.json` com seu usuário/senha do Oracle (o mesmo do
   laboratório da FIAP):
   ```json
   "OracleConnection": "User Id=SEU_RM;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/orcl;"
   ```
4. Instale a ferramenta de migrations (se ainda não tiver):
   ```
   dotnet tool install --global dotnet-ef
   ```
5. Crie e aplique a migration:
   ```
   dotnet ef migrations add PrimeiraMigration
   dotnet ef database update
   ```
6. Rode o projeto:
   ```
   dotnet run
   ```
7. Acesse `https://localhost:{porta}/Livros` ou `/Autores`.

## O que a migration vai criar

- Tabela `Autores`
- Tabela `Livros`
- Tabela `AutoresLivros` (intermediária, com chave composta AutorId + LivroId)
- Chaves estrangeiras e relacionamentos N:N

## Checklist de entrega (Checkpoint 3)

Conforme o enunciado, ainda faltam de sua parte:

- [ ] PrintScreen das classes criadas e do `Program.cs`
- [ ] Vídeo do projeto em funcionamento (link do YouTube)
- [ ] Subir o projeto em `.zip`/`.rar` ou enviar link do repositório git
- [ ] Arquivo `.txt` com nome e RM dos integrantes do grupo (anexado à parte,
      sem compactar junto com o projeto)
