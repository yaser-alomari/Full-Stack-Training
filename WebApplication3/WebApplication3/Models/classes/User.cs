using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.classes
{
    public class User
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(25, MinimumLength = 6)]
        public string Password { get; set; }

        public User() { }
    }
}
