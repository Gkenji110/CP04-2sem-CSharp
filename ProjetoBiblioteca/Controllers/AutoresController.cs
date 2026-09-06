using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoBiblioteca.Dados;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    public class AutoresController : Controller
    {
        // Variável que representa o banco
        private readonly AppDbContext _context;

        public AutoresController(AppDbContext context)
        {
            _context = context;
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
            if (!ModelState.IsValid)
            {
                ViewBag.Livros = new MultiSelectList(_context.Livros.ToList(), "Id", "Titulo");
                return View(autor);
            }

            // Salva o autor no banco
            _context.Autores.Add(autor);
            _context.SaveChanges();

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

            // DESASSOCIA os livros que foram desmarcados
            var paraRemover = associacoesAtuais
                .Where(al => !idsSelecionados.Contains(al.LivroId))
                .ToList();
            _context.AutoresLivros.RemoveRange(paraRemover);

            // ASSOCIA os livros novos que foram marcados
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

            return RedirectToAction("Index");
        }
    }
}
