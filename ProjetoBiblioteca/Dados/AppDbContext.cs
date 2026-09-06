using Microsoft.EntityFrameworkCore;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Dados
{
    public class AppDbContext : DbContext
    {
        // Construtor do contexto
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /*
            Cada DbSet representa uma tabela no banco de dados.
        */
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Livro> Livros { get; set; }
        public DbSet<AutorLivro> AutoresLivros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
                Define chave primária composta.
                A tabela AutorLivro será identificada por:
                - AutorId
                - LivroId
            */
            modelBuilder.Entity<AutorLivro>()
                .HasKey(al => new { al.AutorId, al.LivroId });

            /*
                Configura relação:
                Um Autor possui muitos registros em AutoresLivros
            */
            modelBuilder.Entity<AutorLivro>()
                .HasOne(al => al.Autor)
                .WithMany(a => a.AutoresLivros)
                .HasForeignKey(al => al.AutorId);

            /*
                Configura relação:
                Um Livro possui muitos registros em AutoresLivros
            */
            modelBuilder.Entity<AutorLivro>()
                .HasOne(al => al.Livro)
                .WithMany(l => l.AutoresLivros)
                .HasForeignKey(al => al.LivroId);
        }
    }
}
