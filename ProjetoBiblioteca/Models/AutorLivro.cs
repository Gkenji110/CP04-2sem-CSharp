namespace ProjetoBiblioteca.Models
{
    /*
        Classe intermediária da relação N:N

        Esta tabela serve para conectar:
        - Autores
        - Livros

        Ela guarda:
        qual autor escreveu qual livro
    */
    public class AutorLivro
    {
        // Chave estrangeira para Autor
        public int AutorId { get; set; }

        // Objeto Autor relacionado
        public Autor Autor { get; set; }

        // Chave estrangeira para Livro
        public int LivroId { get; set; }

        // Objeto Livro relacionado
        public Livro Livro { get; set; }
    }
}
