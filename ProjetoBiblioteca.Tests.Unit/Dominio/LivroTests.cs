using FluentAssertions;
using Moq;
using ProjetoBiblioteca.Aplicacao.Servicos;
using ProjetoBiblioteca.Dominio.Interfaces;
using ProjetoBiblioteca.Models;
using Xunit;

namespace ProjetoBiblioteca.Tests.Unit.Dominio
{
    // Suite de testes unitarios para validar as regras de negocio (dominio) do Livro.
    public class LivroTests
    {
        private readonly Mock<ILivroRepositorio> _repositorioMock;
        private readonly LivroServico _servico;

        public LivroTests()
        {
            _repositorioMock = new Mock<ILivroRepositorio>();
            _servico = new LivroServico(_repositorioMock.Object);
        }

        [Fact]
        public void Criar_DadosValidos_DeveCriarComSucesso()
        {
            // Arrange
            var livro = new Livro { Titulo = "Dom Casmurro", AnoPublicacao = 1899, Editora = "Garnier" };

            // Act
            var resultado = _servico.Criar(livro);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Titulo.Should().Be("Dom Casmurro");
            _repositorioMock.Verify(r => r.Adicionar(livro), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Criar_TituloInvalido_DeveLancarArgumentException(string tituloInvalido)
        {
            // Arrange
            var livro = new Livro { Titulo = tituloInvalido, AnoPublicacao = 2000 };

            // Act
            Action acao = () => _servico.Criar(livro);

            // Assert
            acao.Should().Throw<ArgumentException>()
                .WithMessage("*O título do livro é obrigatório*");
            _repositorioMock.Verify(r => r.Adicionar(It.IsAny<Livro>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Criar_AnoPublicacaoInvalido_DeveLancarArgumentException(int anoInvalido)
        {
            // Arrange
            var livro = new Livro { Titulo = "Titulo Valido", AnoPublicacao = anoInvalido };

            // Act
            Action acao = () => _servico.Criar(livro);

            // Assert
            acao.Should().Throw<ArgumentException>()
                .WithMessage("*ano de publicação*");
            _repositorioMock.Verify(r => r.Adicionar(It.IsAny<Livro>()), Times.Never);
        }
    }
}
