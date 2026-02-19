using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using WebApplication3.Models.classes;

namespace MyWebApi.Controllers
{
    public class UserController : Controller
    {
        private static List<User> _users = new List<User> 
        {
            new User {Id = "1",Name = "Ahmad",Email="Ahmad@gmail.com" },
            new User {Id = "2",Name = "Yasser",Email="Yasser@gmail.com" },
        };
        [HttpGet]
        public IActionResult Index() 
        {
            return RedirectToAction(nameof(UserInfo));
        }

        // GET: Display Form
        [HttpGet]
        public IActionResult Create()
        {
            return View( "UserCreate/Index"); // هاد بيرجع Index.cshtml
        }
        [HttpGet]
        public IActionResult UserInfo() 
        {
            return View("UserInfo/Index",_users);
        }

        // POST: Receive Form Data
        [HttpPost]
        public IActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                // مؤقتًا نخزن رسالة في TempData
                TempData["Message"] = "User created successfully!";
                return RedirectToAction("Success");
            }

            user.Id = (_users.Count +1).ToString();
            _users.Add(user);
           
            return RedirectToAction(nameof(UserInfo));
        }

       
    }
}
