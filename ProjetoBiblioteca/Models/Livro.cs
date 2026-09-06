using System.ComponentModel.DataAnnotations;

namespace ProjetoBiblioteca.Models
{
    // Classe que representa um livro no sistema
    public class Livro
    {
        // Chave primária da tabela
        public int Id { get; set; }

        // Campo obrigatório
        [Required(ErrorMessage = "O título do livro é obrigatório")]
        [Display(Name = "Título do Livro")]
        public string Titulo { get; set; }

        [Display(Name = "Ano de Publicação")]
        public int AnoPublicacao { get; set; }

        [Display(Name = "Editora")]
        public string Editora { get; set; }

        // Relação N:N com Autor através da tabela intermediária AutorLivro
        public List<AutorLivro> AutoresLivros { get; set; } = new List<AutorLivro>();
    }
}
