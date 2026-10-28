using System.Diagnostics;
using DemyMath.ViewModels;
using DemyMath.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;


namespace DemyMath.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<SharedResource> _text;

        public HomeController(ILogger<HomeController> logger, IStringLocalizer<SharedResource> text)
        {
            _logger = logger;
            _text = text;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = _text["About"];
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
