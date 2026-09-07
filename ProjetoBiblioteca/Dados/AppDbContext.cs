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
                Nomes de tabelas e colunas em maiúsculas.
                Assim o Oracle guarda os identificadores sem aspas,
                e as queries podem ser escritas de forma normal:
                SELECT * FROM AUTORES; (sem precisar de "Autores")
            */
            modelBuilder.Entity<Autor>(entity =>
            {
                entity.ToTable("AUTORES");
                entity.Property(a => a.Id).HasColumnName("ID");
                entity.Property(a => a.Nome).HasColumnName("NOME");
                entity.Property(a => a.Nacionalidade).HasColumnName("NACIONALIDADE");
            });

            modelBuilder.Entity<Livro>(entity =>
            {
                entity.ToTable("LIVROS");
                entity.Property(l => l.Id).HasColumnName("ID");
                entity.Property(l => l.Titulo).HasColumnName("TITULO");
                entity.Property(l => l.AnoPublicacao).HasColumnName("ANOPUBLICACAO");
                entity.Property(l => l.Editora).HasColumnName("EDITORA");
            });

            modelBuilder.Entity<AutorLivro>(entity =>
            {
                entity.ToTable("AUTORESLIVROS");
                entity.Property(al => al.AutorId).HasColumnName("AUTORID");
                entity.Property(al => al.LivroId).HasColumnName("LIVROID");

                /*
                    Define chave primária composta.
                    A tabela AutorLivro será identificada por:
                    - AutorId
                    - LivroId
                */
                entity.HasKey(al => new { al.AutorId, al.LivroId });

                /*
                    Configura relação:
                    Um Autor possui muitos registros em AutoresLivros
                */
                entity.HasOne(al => al.Autor)
                    .WithMany(a => a.AutoresLivros)
                    .HasForeignKey(al => al.AutorId);

                /*
                    Configura relação:
                    Um Livro possui muitos registros em AutoresLivros
                */
                entity.HasOne(al => al.Livro)
                    .WithMany(l => l.AutoresLivros)
                    .HasForeignKey(al => al.LivroId);
            });
        }
    }
}