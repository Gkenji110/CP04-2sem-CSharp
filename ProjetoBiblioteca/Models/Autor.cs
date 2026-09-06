using System.ComponentModel.DataAnnotations;

namespace ProjetoBiblioteca.Models
{
    // Classe que representa um autor no sistema
    public class Autor
    {
        // Chave primária da tabela
        public int Id { get; set; }

        // Campo obrigatório
        [Required(ErrorMessage = "O nome do autor é obrigatório")]
        [Display(Name = "Nome do Autor")]
        public string Nome { get; set; }

        [Display(Name = "Nacionalidade")]
        public string Nacionalidade { get; set; }

        // Relação N:N com Livro através da tabela intermediária AutorLivro
        public List<AutorLivro> AutoresLivros { get; set; } = new List<AutorLivro>();
    }
}
