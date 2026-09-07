using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Dominio.Interfaces
{
    // Contrato de acesso a dados para a entidade Autor
    public interface IAutorRepositorio
    {
        void Adicionar(Autor autor);
        Autor ObterPorId(int id);
        IEnumerable<Autor> ListarTodos();
    }
}
