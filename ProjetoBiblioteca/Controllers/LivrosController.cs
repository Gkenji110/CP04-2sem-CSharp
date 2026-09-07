using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoBiblioteca.Dados;
using ProjetoBiblioteca.Models;
using ProjetoBiblioteca.Infraestrutura.Observabilidade; // OpenTelemetry
using ProjetoBiblioteca.Aplicacao.Servicos; // Camada de Servico

namespace ProjetoBiblioteca.Controllers
{
    public class LivrosController : Controller
    {
        // Variável que representa o banco
        private readonly AppDbContext _context;
        private readonly ILogger<LivrosController> _logger; // Logging estruturado
        private readonly ILivroServico _livroServico; // Camada de Servico (testavel via Mock)

        public LivrosController(AppDbContext context, ILogger<LivrosController> logger, ILivroServico livroServico)
        {
            _context = context;
            _logger = logger;
            _livroServico = livroServico;
        }

        /*LISTAGEM DE LIVROS*/
        public IActionResult Index()
        {
            /*
                Include -> carrega os relacionamentos
                ThenInclude -> carrega os dados do autor
            */
            var livros = _context.Livros
                .Include(l => l.AutoresLivros)
                .ThenInclude(al => al.Autor)
                .ToList();

            return View(livros);
        }

        /*DETALHES DE UM LIVRO*/
        public IActionResult Details(int id)
        {
            var livro = _context.Livros
                .Include(l => l.AutoresLivros)
                .ThenInclude(al => al.Autor)
                .FirstOrDefault(l => l.Id == id);

            if (livro == null) return NotFound();

            return View(livro);
        }

        /*EXIBE O FORMULÁRIO DE CRIAÇÃO*/
        public IActionResult Create()
        {
            /*MultiSelectList cria uma lista de múltipla seleção.
              O usuário poderá escolher vários autores para o livro.*/
            ViewBag.Autores = new MultiSelectList(
                _context.Autores.ToList(),
                "Id",
                "Nome"
            );

            return View();
        }

        /*RECEBE OS DADOS DO FORMULÁRIO DE CRIAÇÃO*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Livro livro, int[] autoresSelecionados)
        {
            // Inicia um Span customizado via ActivitySource para rastreamento distribuido
            using var activity = AplicacaoMetricas.ActivitySourceAplicacao.StartActivity("CriarLivro");
            activity?.SetTag("livro.titulo", livro?.Titulo);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Tentativa de cadastro de livro com dados invalidos.");
                activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, "Dados invalidos");
                AplicacaoMetricas.LivrosCriadosContador.Add(1,
                    new KeyValuePair<string, object>("status", "erro_validacao"));
                ViewBag.Autores = new MultiSelectList(_context.Autores.ToList(), "Id", "Nome");
                return View(livro);
            }

            try
            {
                // Salva o livro atraves da camada de Servico (aplica as regras de negocio)
                _livroServico.Criar(livro);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro de validacao ao cadastrar livro: {Mensagem}", ex.Message);
                activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
                AplicacaoMetricas.LivrosCriadosContador.Add(1,
                    new KeyValuePair<string, object>("status", "erro_validacao"));
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Autores = new MultiSelectList(_context.Autores.ToList(), "Id", "Nome");
                return View(livro);
            }

            _logger.LogInformation("Livro {LivroId} ({Titulo}) cadastrado com sucesso.", livro.Id, livro.Titulo);

            // Incrementa a metrica de sucesso e finaliza o Span com o id gerado
            activity?.SetTag("livro.id", livro.Id);
            AplicacaoMetricas.LivrosCriadosContador.Add(1,
                new KeyValuePair<string, object>("status", "sucesso"));

            /*
                Percorre todos os autores
                escolhidos pelo usuário.
            */
            if (autoresSelecionados != null)
            {
                foreach (var autorId in autoresSelecionados)
                {
                    // Cria a ligação entre livro e autor
                    var al = new AutorLivro
                    {
                        LivroId = livro.Id,
                        AutorId = autorId
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
            var livro = _context.Livros
                .Include(l => l.AutoresLivros)
                .FirstOrDefault(l => l.Id == id);

            if (livro == null) return NotFound();

            // Ids dos autores já associados a este livro, para pré-selecionar no formulário
            var autoresSelecionadosIds = livro.AutoresLivros.Select(al => al.AutorId).ToList();

            ViewBag.Autores = new MultiSelectList(
                _context.Autores.ToList(),
                "Id",
                "Nome",
                autoresSelecionadosIds
            );

            return View(livro);
        }

        /*RECEBE OS DADOS DO FORMULÁRIO DE EDIÇÃO*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Livro livro, int[] autoresSelecionados)
        {
            if (id != livro.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Autores = new MultiSelectList(_context.Autores.ToList(), "Id", "Nome", autoresSelecionados);
                return View(livro);
            }

            // Atualiza os dados básicos do livro
            _context.Livros.Update(livro);

            // Carrega as associações atuais deste livro
            var associacoesAtuais = _context.AutoresLivros
                .Where(al => al.LivroId == id)
                .ToList();

            var idsSelecionados = autoresSelecionados ?? Array.Empty<int>();

            // DESASSOCIA os autores que foram desmarcados
            var paraRemover = associacoesAtuais
                .Where(al => !idsSelecionados.Contains(al.AutorId))
                .ToList();
            _context.AutoresLivros.RemoveRange(paraRemover);

            // ASSOCIA os autores novos que foram marcados
            var idsAtuais = associacoesAtuais.Select(al => al.AutorId).ToList();
            foreach (var autorId in idsSelecionados)
            {
                if (!idsAtuais.Contains(autorId))
                {
                    _context.AutoresLivros.Add(new AutorLivro
                    {
                        LivroId = id,
                        AutorId = autorId
                    });
                }
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        /*EXIBE A CONFIRMAÇÃO DE EXCLUSÃO*/
        public IActionResult Delete(int id)
        {
            var livro = _context.Livros
                .Include(l => l.AutoresLivros)
                .ThenInclude(al => al.Autor)
                .FirstOrDefault(l => l.Id == id);

            if (livro == null) return NotFound();

            return View(livro);
        }

        /*EXCLUI O LIVRO E SUAS ASSOCIAÇÕES*/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Remove primeiro as associações da tabela intermediária
            var associacoes = _context.AutoresLivros.Where(al => al.LivroId == id);
            _context.AutoresLivros.RemoveRange(associacoes);

            // Remove o livro
            var livro = _context.Livros.Find(id);
            if (livro != null)
            {
                _context.Livros.Remove(livro);
            }

            _context.SaveChanges();
            _logger.LogInformation("Livro {LivroId} removido com sucesso.", id);

            return RedirectToAction("Index");
        }
    }
}
