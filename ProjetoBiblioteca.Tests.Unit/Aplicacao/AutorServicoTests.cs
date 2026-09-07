using FluentAssertions;
using Moq;
using ProjetoBiblioteca.Aplicacao.Servicos;
using ProjetoBiblioteca.Dominio.Interfaces;
using ProjetoBiblioteca.Models;
using Xunit;

namespace ProjetoBiblioteca.Tests.Unit.Aplicacao
{
    // Suite de testes unitarios com Mock de repositorio para a camada de aplicacao (AutorServico).
    public class AutorServicoTests
    {
        private readonly Mock<IAutorRepositorio> _repositorioMock;
        private readonly AutorServico _servico;

        public AutorServicoTests()
        {
            _repositorioMock = new Mock<IAutorRepositorio>();
            _servico = new AutorServico(_repositorioMock.Object);
        }

        [Fact]
        public void BuscarPorId_AutorExistente_DeveRetornarAutorEsperado()
        {
            // Arrange
            var autorExistente = new Autor { Id = 1, Nome = "Clarice Lispector" };
            _repositorioMock.Setup(r => r.ObterPorId(1)).Returns(autorExistente);

            // Act
            var resultado = _servico.BuscarPorId(1);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Clarice Lispector");
            _repositorioMock.Verify(r => r.ObterPorId(1), Times.Once);
        }

        [Fact]
        public void BuscarPorId_AutorInexistente_DeveRetornarNulo()
        {
            // Arrange
            _repositorioMock.Setup(r => r.ObterPorId(99)).Returns((Autor)null);

            // Act
            var resultado = _servico.BuscarPorId(99);

            // Assert
            resultado.Should().BeNull();
            _repositorioMock.Verify(r => r.ObterPorId(99), Times.Once);
        }

        [Fact]
        public void ListarTodos_DeveRetornarTodosOsAutoresDoRepositorio()
        {
            // Arrange
            var autores = new List<Autor>
            {
                new Autor { Id = 1, Nome = "Autor 1" },
                new Autor { Id = 2, Nome = "Autor 2" }
            };
            _repositorioMock.Setup(r => r.ListarTodos()).Returns(autores);

            // Act
            var resultado = _servico.ListarTodos();

            // Assert
            resultado.Should().HaveCount(2);
            _repositorioMock.Verify(r => r.ListarTodos(), Times.Once);
        }
    }
}
