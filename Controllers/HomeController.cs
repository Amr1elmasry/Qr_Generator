using Microsoft.AspNetCore.Mvc;
using Qr_Generator.Models;
using System.Diagnostics;

namespace Qr_Generator.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {

        }

        public IActionResult Index()
        {
            return View();
        }



        
    }
}