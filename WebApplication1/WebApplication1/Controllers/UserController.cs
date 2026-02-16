using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.classes;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            User user = new User
            {
                Id = "1",
                Name = "Yasser",
                Email = "yasser@email.com"
            };

            return View(user);
        }
    }
}
