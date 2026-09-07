using FluentAssertions;
using Moq;
using ProjetoBiblioteca.Aplicacao.Servicos;
using ProjetoBiblioteca.Dominio.Interfaces;
using ProjetoBiblioteca.Models;
using Xunit;

namespace ProjetoBiblioteca.Tests.Unit.Aplicacao
{
    // Suite de testes unitarios com Mock de repositorio para a camada de aplicacao (LivroServico).
    public class LivroServicoTests
    {
        private readonly Mock<ILivroRepositorio> _repositorioMock;
        private readonly LivroServico _servico;

        public LivroServicoTests()
        {
            _repositorioMock = new Mock<ILivroRepositorio>();
            _servico = new LivroServico(_repositorioMock.Object);
        }

        [Fact]
        public void BuscarPorId_LivroExistente_DeveRetornarLivroEsperado()
        {
            // Arrange
            var livroExistente = new Livro { Id = 1, Titulo = "O Cortiço", AnoPublicacao = 1890 };
            _repositorioMock.Setup(r => r.ObterPorId(1)).Returns(livroExistente);

            // Act
            var resultado = _servico.BuscarPorId(1);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Titulo.Should().Be("O Cortiço");
            _repositorioMock.Verify(r => r.ObterPorId(1), Times.Once);
        }

        [Fact]
        public void BuscarPorId_LivroInexistente_DeveRetornarNulo()
        {
            // Arrange
            _repositorioMock.Setup(r => r.ObterPorId(99)).Returns((Livro)null);

            // Act
            var resultado = _servico.BuscarPorId(99);

            // Assert
            resultado.Should().BeNull();
            _repositorioMock.Verify(r => r.ObterPorId(99), Times.Once);
        }

        [Fact]
        public void Criar_DadosValidos_DeveChamarRepositorioExatamenteUmaVez()
        {
            // Arrange
            var livro = new Livro { Titulo = "Grande Sertão: Veredas", AnoPublicacao = 1956 };

            // Act
            _servico.Criar(livro);

            // Assert
            _repositorioMock.Verify(r => r.Adicionar(It.Is<Livro>(l => l.Titulo == livro.Titulo)), Times.Once);
        }
    }
}
