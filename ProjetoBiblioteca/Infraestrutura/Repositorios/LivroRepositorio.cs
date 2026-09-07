using ProjetoBiblioteca.Dados;
using ProjetoBiblioteca.Dominio.Interfaces;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Infraestrutura.Repositorios
{
    // Implementacao do repositorio de Livro utilizando o Entity Framework Core / Oracle
    public class LivroRepositorio : ILivroRepositorio
    {
        private readonly AppDbContext _context;

        public LivroRepositorio(AppDbContext context)
        {
            _context = context;
        }

        public void Adicionar(Livro livro)
        {
            _context.Livros.Add(livro);
            _context.SaveChanges();
        }

        public Livro ObterPorId(int id)
        {
            return _context.Livros.Find(id);
        }

        public IEnumerable<Livro> ListarTodos()
        {
            return _context.Livros.ToList();
        }
    }
}
