using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

//using MyWebApi.Models;
using WebApplication2.Models;

namespace MyWebApi.Controllers
{
    
    public class UserController : Controller
    {
        
        public IActionResult Index()
        {
            User user = new User
            {
                Id = 1,
                Name = "ahmad",
                Email = "ahmad@gmail.com"
            };
                
                

            return View(user);
        }

        
    }
}