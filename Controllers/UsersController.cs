using Microsoft.AspNetCore.Mvc;
using Qr_Generator.Models;

namespace Qr_Generator.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public UsersController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var users = _dbContext.Users;
            if (users == null) return View();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(string Name)
        {
            var user = new User
            {
                Name = Name
            };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Profile(int id)
        {
            var user = _dbContext.Users.Find(id);
            if (user == null) return View();
            return View(user);
        }

        
    }
}
