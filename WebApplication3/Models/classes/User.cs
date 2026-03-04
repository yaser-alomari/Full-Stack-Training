using System.ComponentModel.DataAnnotations;

namespace Models.classes
{
    public class User
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(25, MinimumLength = 6,
            ErrorMessage = "Password must be between 6 and 25 characters")]
        public string? Password { get; set; }

        public User() { }
    }
}









//using System.ComponentModel.DataAnnotations;

//namespace WebApplication3.Models.classes
//{
//    public class User
//    {
//        public string? Id { get; set; }

//        [Required(ErrorMessage = "Name is required")]
//        public string Name { get; set; }
//        [Required(ErrorMessage = "Email is required")]
//        public string Email { get; set; }

//        [Required(ErrorMessage = "Password is required")]
//        [StringLength(25, MinimumLength = 6)]
//        public string Password { get; set; }

//        public User() { }
//    }
//}
