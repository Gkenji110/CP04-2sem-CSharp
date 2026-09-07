using ProjetoBiblioteca.Dominio.Interfaces;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Aplicacao.Servicos
{
    // Camada de aplicacao responsavel pelas regras de negocio do Autor,
    // independente de EF Core/Oracle/MVC (por isso e facil de testar com Mock).
    public class AutorServico : IAutorServico
    {
        private readonly IAutorRepositorio _repositorio;

        public AutorServico(IAutorRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public Autor Criar(Autor autor)
        {
            if (autor is null)
            {
                throw new ArgumentException("O autor informado é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(autor.Nome))
            {
                throw new ArgumentException("O nome do autor é obrigatório.");
            }

            _repositorio.Adicionar(autor);

            return autor;
        }

        public Autor BuscarPorId(int id)
        {
            return _repositorio.ObterPorId(id);
        }

        public IEnumerable<Autor> ListarTodos()
        {
            return _repositorio.ListarTodos();
        }
    }
}
