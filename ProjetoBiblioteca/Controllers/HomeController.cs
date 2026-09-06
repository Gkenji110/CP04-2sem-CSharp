using Microsoft.AspNetCore.Mvc;

namespace ProjetoBiblioteca.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Livros");
        }

        public IActionResult Error()
        {
            return Content("Ocorreu um erro.");
        }
    }
}
