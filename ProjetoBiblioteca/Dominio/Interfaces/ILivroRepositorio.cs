using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Dominio.Interfaces
{
    // Contrato de acesso a dados para a entidade Livro
    public interface ILivroRepositorio
    {
        void Adicionar(Livro livro);
        Livro ObterPorId(int id);
        IEnumerable<Livro> ListarTodos();
    }
}
