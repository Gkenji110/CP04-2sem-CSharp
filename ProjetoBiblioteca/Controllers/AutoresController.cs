using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoBiblioteca.Dados;
using ProjetoBiblioteca.Models;
using ProjetoBiblioteca.Infraestrutura.Observabilidade; // OpenTelemetry
using ProjetoBiblioteca.Aplicacao.Servicos; // Camada de Servico

namespace ProjetoBiblioteca.Controllers
{
    public class AutoresController : Controller
    {
        // Variável que representa o banco
        private readonly AppDbContext _context;
        private readonly ILogger<AutoresController> _logger; // Logging estruturado
        private readonly IAutorServico _autorServico; // Camada de Servico (testavel via Mock)

        public AutoresController(AppDbContext context, ILogger<AutoresController> logger, IAutorServico autorServico)
        {
            _context = context;
            _logger = logger;
            _autorServico = autorServico;
        }

        /*LISTAGEM DE AUTORES*/
        public IActionResult Index()
        {
            /*
                Include -> carrega os relacionamentos
                ThenInclude -> carrega os dados do livro
            */
            var autores = _context.Autores
                .Include(a => a.AutoresLivros)
                .ThenInclude(al => al.Livro)
                .ToList();

            return View(autores);
        }

        /*DETALHES DE UM AUTOR*/
        public IActionResult Details(int id)
        {
            var autor = _context.Autores
                .Include(a => a.AutoresLivros)
                .ThenInclude(al => al.Livro)
                .FirstOrDefault(a => a.Id == id);

            if (autor == null) return NotFound();

            return View(autor);
        }

        /*EXIBE O FORMULÁRIO DE CRIAÇÃO*/
        public IActionResult Create()
        {
            /*MultiSelectList cria uma lista de múltipla seleção.
              O usuário poderá escolher vários livros para o autor.*/
            ViewBag.Livros = new MultiSelectList(
                _context.Livros.ToList(),
                "Id",
                "Titulo"
            );

            return View();
        }

        /*RECEBE OS DADOS DO FORMULÁRIO DE CRIAÇÃO*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Autor autor, int[] livrosSelecionados)
        {
            // Inicia um Span customizado via ActivitySource para rastreamento distribuido
            using var activity = AplicacaoMetricas.ActivitySourceAplicacao.StartActivity("CriarAutor");
            activity?.SetTag("autor.nome", autor?.Nome);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Tentativa de cadastro de autor com dados invalidos.");
                activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, "Dados invalidos");
                AplicacaoMetricas.AutoresCriadosContador.Add(1,
                    new KeyValuePair<string, object>("status", "erro_validacao"));
                ViewBag.Livros = new MultiSelectList(_context.Livros.ToList(), "Id", "Titulo");
                return View(autor);
            }

            try
            {
                // Salva o autor atraves da camada de Servico (aplica as regras de negocio)
                _autorServico.Criar(autor);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro de validacao ao cadastrar autor: {Mensagem}", ex.Message);
                activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
                AplicacaoMetricas.AutoresCriadosContador.Add(1,
                    new KeyValuePair<string, object>("status", "erro_validacao"));
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Livros = new MultiSelectList(_context.Livros.ToList(), "Id", "Titulo");
                return View(autor);
            }

            _logger.LogInformation("Autor {AutorId} ({Nome}) cadastrado com sucesso.", autor.Id, autor.Nome);

            // Incrementa a metrica de sucesso e finaliza o Span com o id gerado
            activity?.SetTag("autor.id", autor.Id);
            AplicacaoMetricas.AutoresCriadosContador.Add(1,
                new KeyValuePair<string, object>("status", "sucesso"));

            /*
                Percorre todos os livros
                escolhidos pelo usuário.
            */
            if (livrosSelecionados != null)
            {
                foreach (var livroId in livrosSelecionados)
                {
                    // Cria a ligação entre autor e livro
                    var al = new AutorLivro
                    {
                        AutorId = autor.Id,
                        LivroId = livroId
                    };

                    // Adiciona no banco
                    _context.AutoresLivros.Add(al);
                }

                // Salva tudo
                _context.SaveChanges();
            }

            // Redireciona para Index
            return RedirectToAction("Index");
        }

        /*EXIBE O FORMULÁRIO DE EDIÇÃO*/
        public IActionResult Edit(int id)
        {
            var autor = _context.Autores
                .Include(a => a.AutoresLivros)
                .FirstOrDefault(a => a.Id == id);

            if (autor == null) return NotFound();

            // Ids dos livros já associados a este autor, para pré-selecionar no formulário
            var livrosSelecionadosIds = autor.AutoresLivros.Select(al => al.LivroId).ToList();

            ViewBag.Livros = new MultiSelectList(
                _context.Livros.ToList(),
                "Id",
                "Titulo",
                livrosSelecionadosIds
            );

            return View(autor);
        }

        /*RECEBE OS DADOS DO FORMULÁRIO DE EDIÇÃO*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Autor autor, int[] livrosSelecionados)
        {
            if (id != autor.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Livros = new MultiSelectList(_context.Livros.ToList(), "Id", "Titulo", livrosSelecionados);
                return View(autor);
            }

            // Atualiza os dados básicos do autor
            _context.Autores.Update(autor);

            // Carrega as associações atuais deste autor
            var associacoesAtuais = _context.AutoresLivros
                .Where(al => al.AutorId == id)
                .ToList();

            var idsSelecionados = livrosSelecionados ?? Array.Empty<int>();

            // Remove os livros que foram desmarcados
            var paraRemover = associacoesAtuais
                .Where(al => !idsSelecionados.Contains(al.LivroId))
                .ToList();
            _context.AutoresLivros.RemoveRange(paraRemover);

            // Adiciona os livros novos que foram marcados
            var idsAtuais = associacoesAtuais.Select(al => al.LivroId).ToList();
            foreach (var livroId in idsSelecionados)
            {
                if (!idsAtuais.Contains(livroId))
                {
                    _context.AutoresLivros.Add(new AutorLivro
                    {
                        AutorId = id,
                        LivroId = livroId
                    });
                }
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        /*EXIBE A CONFIRMAÇÃO DE EXCLUSÃO*/
        public IActionResult Delete(int id)
        {
            var autor = _context.Autores
                .Include(a => a.AutoresLivros)
                .ThenInclude(al => al.Livro)
                .FirstOrDefault(a => a.Id == id);

            if (autor == null) return NotFound();

            return View(autor);
        }

        /*EXCLUI O AUTOR E SUAS ASSOCIAÇÕES*/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Remove primeiro as associações da tabela intermediária
            var associacoes = _context.AutoresLivros.Where(al => al.AutorId == id);
            _context.AutoresLivros.RemoveRange(associacoes);

            // Remove o autor
            var autor = _context.Autores.Find(id);
            if (autor != null)
            {
                _context.Autores.Remove(autor);
            }

            _context.SaveChanges();
            _logger.LogInformation("Autor {AutorId} removido com sucesso.", id);

            return RedirectToAction("Index");
        }
    }
}
