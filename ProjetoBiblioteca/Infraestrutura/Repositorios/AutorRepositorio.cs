using ProjetoBiblioteca.Dados;
using ProjetoBiblioteca.Dominio.Interfaces;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Infraestrutura.Repositorios
{
    // Implementacao do repositorio de Autor utilizando o Entity Framework Core / Oracle
    public class AutorRepositorio : IAutorRepositorio
    {
        private readonly AppDbContext _context;

        public AutorRepositorio(AppDbContext context)
        {
            _context = context;
        }

        public void Adicionar(Autor autor)
        {
            _context.Autores.Add(autor);
            _context.SaveChanges();
        }

        public Autor ObterPorId(int id)
        {
            return _context.Autores.Find(id);
        }

        public IEnumerable<Autor> ListarTodos()
        {
            return _context.Autores.ToList();
        }
    }
}
