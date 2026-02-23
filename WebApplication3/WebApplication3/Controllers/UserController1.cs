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
            new User {Id = "1",Name = "Ahmad",Email="Ahmad@gmail.com",Password="123456" },
            new User {Id = "2",Name = "Yasser",Email="Yasser@gmail.com",Password="654321" },
        };
        [HttpGet]
        public IActionResult Index() 
        {
            return RedirectToAction(nameof(Login));
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

             int nextid = _users
            .Select(u => int.TryParse(u.Id, out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;
            user.Id = (nextid).ToString();
            if (!ModelState.IsValid)
            {

                return View("UserCreate/Index");
            }
            _users.Add(user);

                TempData["SuccessMessage"] = "User added successfully!";
                return RedirectToAction(nameof(UserInfo));
            
        }
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);

            if (user != null)
            {
                _users.Remove(user);
                TempData["SuccessMessage"] = "User deleted successfully!";
            }

            return RedirectToAction(nameof(UserInfo));
        }

        [HttpPost]
        public IActionResult Edit(User updatedUser)
        {
            var user = _users.FirstOrDefault(u => u.Id == updatedUser.Id);

            if (user == null)
                return NotFound();

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;

            TempData["SuccessMessage"] = "User updated successfully!";

            return RedirectToAction(nameof(UserInfo));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View("Login/Index");
        }

        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            // تحقق من وجود مستخدم بنفس البريد وكلمة السر
            var user = _users.FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user != null)
            {
                // تسجيل الدخول ناجح
                TempData["SuccessMessage"] = $"Welcome {user.Name}!";
                return RedirectToAction(nameof(UserInfo));
            }
            else
            {
                // خطأ
                TempData["ErrorMessage"] = "Email or Password is incorrect!";
                return View("Login/Index");
            }
        }

    }
}
