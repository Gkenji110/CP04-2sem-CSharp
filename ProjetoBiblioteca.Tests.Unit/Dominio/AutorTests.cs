using FluentAssertions;
using Moq;
using ProjetoBiblioteca.Aplicacao.Servicos;
using ProjetoBiblioteca.Dominio.Interfaces;
using ProjetoBiblioteca.Models;
using Xunit;

namespace ProjetoBiblioteca.Tests.Unit.Dominio
{
    // Suite de testes unitarios para validar as regras de negocio (dominio) do Autor.
    // A validacao fica na camada de Aplicacao (AutorServico), entao o repositorio
    // e mockado apenas para permitir a instanciacao do servico - ele nao deve ser
    // chamado quando os dados forem invalidos.
    public class AutorTests
    {
        private readonly Mock<IAutorRepositorio> _repositorioMock;
        private readonly AutorServico _servico;

        public AutorTests()
        {
            _repositorioMock = new Mock<IAutorRepositorio>();
            _servico = new AutorServico(_repositorioMock.Object);
        }

        [Fact]
        public void Criar_DadosValidos_DeveCriarComSucesso()
        {
            // Arrange
            var autor = new Autor { Nome = "Machado de Assis", Nacionalidade = "Brasileira" };

            // Act
            var resultado = _servico.Criar(autor);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Machado de Assis");
            _repositorioMock.Verify(r => r.Adicionar(autor), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Criar_NomeInvalido_DeveLancarArgumentException(string nomeInvalido)
        {
            // Arrange
            var autor = new Autor { Nome = nomeInvalido, Nacionalidade = "Brasileira" };

            // Act
            Action acao = () => _servico.Criar(autor);

            // Assert
            acao.Should().Throw<ArgumentException>()
                .WithMessage("*O nome do autor é obrigatório*");
            _repositorioMock.Verify(r => r.Adicionar(It.IsAny<Autor>()), Times.Never);
        }

        [Fact]
        public void Criar_AutorNulo_DeveLancarArgumentException()
        {
            // Arrange
            Autor autor = null;

            // Act
            Action acao = () => _servico.Criar(autor);

            // Assert
            acao.Should().Throw<ArgumentException>();
            _repositorioMock.Verify(r => r.Adicionar(It.IsAny<Autor>()), Times.Never);
        }
    }
}
