using Microsoft.AspNetCore.Mvc;

namespace SugarTalk.Api.Controllers
{
    public class HomeController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Room()
        {
            return View();
        }
    }
}