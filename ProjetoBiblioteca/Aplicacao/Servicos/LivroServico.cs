using ProjetoBiblioteca.Dominio.Interfaces;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Aplicacao.Servicos
{
    // Camada de aplicacao responsavel pelas regras de negocio do Livro,
    // independente de EF Core/Oracle/MVC (por isso e facil de testar com Mock).
    public class LivroServico : ILivroServico
    {
        private readonly ILivroRepositorio _repositorio;

        public LivroServico(ILivroRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public Livro Criar(Livro livro)
        {
            if (livro is null)
            {
                throw new ArgumentException("O livro informado é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(livro.Titulo))
            {
                throw new ArgumentException("O título do livro é obrigatório.");
            }

            if (livro.AnoPublicacao <= 0)
            {
                throw new ArgumentException("O ano de publicação deve ser maior que zero.");
            }

            _repositorio.Adicionar(livro);

            return livro;
        }

        public Livro BuscarPorId(int id)
        {
            return _repositorio.ObterPorId(id);
        }

        public IEnumerable<Livro> ListarTodos()
        {
            return _repositorio.ListarTodos();
        }
    }
}
