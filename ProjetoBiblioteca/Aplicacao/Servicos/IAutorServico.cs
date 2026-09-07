using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Aplicacao.Servicos
{
    public interface IAutorServico
    {
        // Valida as regras de negocio e persiste um novo Autor.
        // Lanca ArgumentException quando os dados sao invalidos.
        Autor Criar(Autor autor);

        Autor BuscarPorId(int id);

        IEnumerable<Autor> ListarTodos();
    }
}
