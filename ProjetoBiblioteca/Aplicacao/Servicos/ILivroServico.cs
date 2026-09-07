using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Aplicacao.Servicos
{
    public interface ILivroServico
    {
        // Valida as regras de negocio e persiste um novo Livro.
        // Lanca ArgumentException quando os dados sao invalidos.
        Livro Criar(Livro livro);

        Livro BuscarPorId(int id);

        IEnumerable<Livro> ListarTodos();
    }
}
