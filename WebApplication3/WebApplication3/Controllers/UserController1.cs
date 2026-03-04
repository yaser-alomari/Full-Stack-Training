using DAL.AccessPoints;
using Microsoft.AspNetCore.Mvc;
using Models.classes;
using System.Text;
using System.Security.Cryptography;


namespace MyWebApi.Controllers
{
    public class UserController : Controller
    {
        private readonly User_Context _userContext;
        private static List<User> _users;
        public UserController(IConfiguration configuration)
        {
            // إنشاء instance من DAL
            _userContext = new User_Context(configuration);
            _users = _userContext.GetAllUsers();

        }
        public IActionResult GetUsers()
        {
            // استخدم _users بدل إعادة الجلب
            return View(_users);
        } 
        

        // ======================
        // Default Page
        // ======================
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Login));
        }

        // ======================
        // CREATE (GET)
        // ======================
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsLoggedIn())
                return RedirectToAction(nameof(Login));

            return View("UserCreate/Index");
        }

        // ======================
        // CREATE (POST)
        // ======================
        [HttpPost]
        public IActionResult Create(User user)
        {
            if (!IsLoggedIn())
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
                return View("UserCreate/Index", user);

            // 🔐 تشفير الباسورد قبل الحفظ
            user.Password = HashPassword(user.Password);

            // 🗄️ إدخال المستخدم في قاعدة البيانات
            _userContext.InsertUser(user);

            TempData["SuccessMessage"] = "User added successfully!";
            return RedirectToAction(nameof(UserInfo));
        }

        // ======================
        // USER INFO
        // ======================
        [HttpGet]
        public IActionResult UserInfo()
        {
            List<User> users = _userContext.GetAllUsers();

            if (!IsLoggedIn())
                return RedirectToAction(nameof(Login));

            return View("UserInfo/Index", _users);
        }

        // ======================
        // DELETE
        // ======================
        [HttpPost]
        public IActionResult Delete(string id)
        {
            try
            {
                // استدعاء الدالة DeleteUser من DAL مع تحويل id إلى long
                _userContext.DeleteUser(id); // داخل DeleteUser يتم Convert.ToInt64

                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            catch
            {
                TempData["ErrorMessage"] = "Failed to delete user. Invalid ID or database error!";
            }

            return RedirectToAction(nameof(UserInfo));
        }

        // ======================
        // EDIT
        // ======================

        [HttpPost]
        public IActionResult Edit(User updatedUser)
        {
            ModelState.Remove("Password");
            if (!ModelState.IsValid)
            {
                // عرض الفورم مرة ثانية مع البيانات المدخلة بدل Redirect
                return RedirectToAction(nameof(UserInfo));
            }

            // تحديث المستخدم مباشرة باستخدام DAL و Stored Procedure
            _userContext.UpdateUser(updatedUser);

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction(nameof(UserInfo));
        }

        // ======================
        // LOGIN (GET)
        // ======================
        [HttpGet]
        public IActionResult Login()
        {
            return View("Login/Index");
        }

        // ======================
        // LOGIN (POST)
        // ======================
        
        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            string hashedPassword = HashPassword(Password);
            var user = _users
                .FirstOrDefault(u => u.Email == Email && u.Password == hashedPassword);

            if (user != null)
            {
                HttpContext.Session.SetString("UserName", user.Name);

                TempData["SuccessMessage"] = $"Welcome {user.Name}!";
                return RedirectToAction(nameof(UserInfo));
            }

            TempData["ErrorMessage"] = "Email or Password is incorrect!";
            return View("Login/Index");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserName") != null;
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}









