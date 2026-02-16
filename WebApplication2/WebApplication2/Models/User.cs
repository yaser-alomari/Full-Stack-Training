using Microsoft.AspNetCore.Components.Forms;

namespace WebApplication2.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public User() { }

    }
}
