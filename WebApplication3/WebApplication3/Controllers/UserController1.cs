using DAL.AccessPoints;
using Microsoft.AspNetCore.Mvc;
using Models.classes;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace MyWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly User_Context _userContext;

        public UserController(IConfiguration configuration)
        {
            _userContext = new User_Context(configuration);
        }

        // =========================
        // Users العامة
        // =========================

        [HttpGet("all")]
        public IActionResult GetUsers()
        {
            var users = _userContext.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var user = _userContext.GetUserById(id);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        [HttpGet("search")]
        public IActionResult Search(string query)
        {
            if (!IsLoggedIn())
                return Unauthorized(new { message = "User is not logged in" });

            if (string.IsNullOrWhiteSpace(query))
                return Ok(_userContext.GetAllUsers());

            var usersList = _userContext.SearchUsers(query);
            return Ok(usersList);
        }

        // =========================
        // Login / Logout / Session
        // =========================

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and password are required" });

            string hashedPassword = HashPassword(request.Password);

            var user = _userContext.Login(request.Email, hashedPassword);

            if (user == null)
                return Unauthorized(new { message = "Email or Password is incorrect!" });

            HttpContext.Session.SetString("UserName", user.Name ?? "");
            HttpContext.Session.SetString("UserId", user.Id ?? "");
            HttpContext.Session.SetString("RoleName", user.RoleName ?? "");

            return Ok(new
            {
                message = $"Welcome {user.Name}!",
                userId = user.Id,
                userName = user.Name,
                email = user.Email,
                roleId = user.RoleId,
                roleName = user.RoleName
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("userinfo")]
        public IActionResult UserInfo()
        {
            if (!IsLoggedIn())
                return Unauthorized(new { message = "User is not logged in" });

            return Ok(new
            {
                userId = HttpContext.Session.GetString("UserId"),
                userName = HttpContext.Session.GetString("UserName"),
                roleName = HttpContext.Session.GetString("RoleName")
            });
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            if (!IsLoggedIn())
                return Unauthorized(new { message = "User is not logged in" });

            var userIdValue = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
                return BadRequest(new { message = "Invalid session user id" });

            var profile = _userContext.GetUserProfile(userId);

            if (profile == null)
                return NotFound(new { message = "Profile not found" });

            return Ok(profile);
        }

        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!IsLoggedIn())
                return Unauthorized(new { message = "User is not logged in" });

            if (request == null ||
                string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Current password and new password are required" });
            }

            var userIdValue = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
                return BadRequest(new { message = "Invalid session user id" });

            string currentHashedPassword = HashPassword(request.CurrentPassword);
            string newHashedPassword = HashPassword(request.NewPassword);

            var result = _userContext.ChangePassword(userId, currentHashedPassword, newHashedPassword);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpGet("check-session")]
        public IActionResult CheckSession()
        {
            if (!IsLoggedIn())
                return Unauthorized(new { message = "No active session" });

            return Ok(new
            {
                userId = HttpContext.Session.GetString("UserId"),
                userName = HttpContext.Session.GetString("UserName"),
                roleName = HttpContext.Session.GetString("RoleName")
            });
        }

        // =========================
        // Doctors
        // =========================

        [HttpGet("doctors/all")]
        public IActionResult GetAllDoctors()
        {
            return Ok(_userContext.GetAllDoctors());
        }

        [HttpGet("doctors/{userId}")]
        public IActionResult GetDoctorByUserId(int userId)
        {
            var doctor = _userContext.GetDoctorByUserId(userId);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            return Ok(doctor);
        }

        [HttpPost("doctors/create")]
        public IActionResult CreateDoctor([FromBody] Doctor doctor)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            doctor.PasswordHash = HashPassword(doctor.PasswordHash);
            _userContext.AddDoctor(doctor);

            return Ok(new { message = "Doctor added successfully" });
        }

        [HttpPut("doctors/edit")]
        public IActionResult EditDoctor([FromBody] Doctor doctor)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            doctor.PasswordHash = HashPassword(doctor.PasswordHash);
            _userContext.UpdateDoctor(doctor);

            return Ok(new { message = "Doctor updated successfully" });
        }

        [HttpDelete("doctors/delete/{userId}")]
        public IActionResult DeleteDoctor(int userId)
        {
            _userContext.DeleteDoctor(userId);
            return Ok(new { message = "Doctor deleted successfully" });
        }

        // =========================
        // Patients
        // =========================

        [HttpGet("patients/all")]
        public IActionResult GetAllPatients()
        {
            return Ok(_userContext.GetAllPatients());
        }

        [HttpGet("patients/{userId}")]
        public IActionResult GetPatientByUserId(int userId)
        {
            var patient = _userContext.GetPatientByUserId(userId);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });

            return Ok(patient);
        }

        [HttpPost("patients/create")]
        public IActionResult CreatePatient([FromBody] Patient patient)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            patient.PasswordHash = HashPassword(patient.PasswordHash);
            _userContext.AddPatient(patient);

            return Ok(new { message = "Patient added successfully" });
        }

        [HttpPut("patients/edit")]
        public IActionResult EditPatient([FromBody] Patient patient)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            patient.PasswordHash = HashPassword(patient.PasswordHash);
            _userContext.UpdatePatient(patient);

            return Ok(new { message = "Patient updated successfully" });
        }

        [HttpDelete("patients/delete/{userId}")]
        public IActionResult DeletePatient(int userId)
        {
            _userContext.DeletePatient(userId);
            return Ok(new { message = "Patient deleted successfully" });
        }

        // =========================
        // Admins
        // =========================

        [HttpGet("admins/all")]
        public IActionResult GetAllAdmins()
        {
            return Ok(_userContext.GetAllAdmins());
        }

        [HttpGet("admins/{userId}")]
        public IActionResult GetAdminByUserId(int userId)
        {
            var admin = _userContext.GetAdminByUserId(userId);
            if (admin == null)
                return NotFound(new { message = "Admin not found" });

            return Ok(admin);
        }

        [HttpPost("admins/create")]
        public IActionResult CreateAdmin([FromBody] Admin admin)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            admin.PasswordHash = HashPassword(admin.PasswordHash);
            _userContext.AddAdmin(admin);

            return Ok(new { message = "Admin added successfully" });
        }

        [HttpPut("admins/edit")]
        public IActionResult EditAdmin([FromBody] Admin admin)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            admin.PasswordHash = HashPassword(admin.PasswordHash);
            _userContext.UpdateAdmin(admin);

            return Ok(new { message = "Admin updated successfully" });
        }

        [HttpDelete("admins/delete/{userId}")]
        public IActionResult DeleteAdmin(int userId)
        {
            _userContext.DeleteAdmin(userId);
            return Ok(new { message = "Admin deleted successfully" });
        }

        // =========================
        // Finance
        // =========================

        [HttpGet("finance/all")]
        public IActionResult GetAllFinance()
        {
            return Ok(_userContext.GetAllFinance());
        }

        [HttpGet("finance/{userId}")]
        public IActionResult GetFinanceByUserId(int userId)
        {
            var finance = _userContext.GetFinanceByUserId(userId);
            if (finance == null)
                return NotFound(new { message = "Finance user not found" });

            return Ok(finance);
        }

        [HttpPost("finance/create")]
        public IActionResult CreateFinance([FromBody] Finance finance)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            finance.PasswordHash = HashPassword(finance.PasswordHash);
            _userContext.AddFinance(finance);

            return Ok(new { message = "Finance user added successfully" });
        }

        [HttpPut("finance/edit")]
        public IActionResult EditFinance([FromBody] Finance finance)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            finance.PasswordHash = HashPassword(finance.PasswordHash);
            _userContext.UpdateFinance(finance);

            return Ok(new { message = "Finance user updated successfully" });
        }

        [HttpDelete("finance/delete/{userId}")]
        public IActionResult DeleteFinance(int userId)
        {
            _userContext.DeleteFinance(userId);
            return Ok(new { message = "Finance user deleted successfully" });
        }

        // =========================
        // Secretaries
        // =========================

        [HttpGet("secretaries/all")]
        public IActionResult GetAllSecretaries()
        {
            return Ok(_userContext.GetAllSecretaries());
        }

        [HttpGet("secretaries/{userId}")]
        public IActionResult GetSecretaryByUserId(int userId)
        {
            var secretary = _userContext.GetSecretaryByUserId(userId);
            if (secretary == null)
                return NotFound(new { message = "Secretary not found" });

            return Ok(secretary);
        }

        [HttpPost("secretaries/create")]
        public IActionResult CreateSecretary([FromBody] Secretary secretary)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            secretary.PasswordHash = HashPassword(secretary.PasswordHash);
            _userContext.AddSecretary(secretary);

            return Ok(new { message = "Secretary added successfully" });
        }

        [HttpPut("secretaries/edit")]
        public IActionResult EditSecretary([FromBody] Secretary secretary)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            secretary.PasswordHash = HashPassword(secretary.PasswordHash);
            _userContext.UpdateSecretary(secretary);

            return Ok(new { message = "Secretary updated successfully" });
        }

        [HttpDelete("secretaries/delete/{userId}")]
        public IActionResult DeleteSecretary(int userId)
        {
            _userContext.DeleteSecretary(userId);
            return Ok(new { message = "Secretary deleted successfully" });
        }

        // =========================
        // Warehouse Managers
        // =========================

        [HttpGet("warehouse-managers/all")]
        public IActionResult GetAllWarehouseManagers()
        {
            return Ok(_userContext.GetAllWarehouseManagers());
        }

        [HttpGet("warehouse-managers/{userId}")]
        public IActionResult GetWarehouseManagerByUserId(int userId)
        {
            var manager = _userContext.GetWarehouseManagerByUserId(userId);
            if (manager == null)
                return NotFound(new { message = "Warehouse manager not found" });

            return Ok(manager);
        }

        [HttpPost("warehouse-managers/create")]
        public IActionResult CreateWarehouseManager([FromBody] WarehouseManager warehouseManager)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            warehouseManager.PasswordHash = HashPassword(warehouseManager.PasswordHash);
            _userContext.AddWarehouseManager(warehouseManager);

            return Ok(new { message = "Warehouse manager added successfully" });
        }

        [HttpPut("warehouse-managers/edit")]
        public IActionResult EditWarehouseManager([FromBody] WarehouseManager warehouseManager)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            warehouseManager.PasswordHash = HashPassword(warehouseManager.PasswordHash);
            _userContext.UpdateWarehouseManager(warehouseManager);

            return Ok(new { message = "Warehouse manager updated successfully" });
        }

        [HttpDelete("warehouse-managers/delete/{userId}")]
        public IActionResult DeleteWarehouseManager(int userId)
        {
            _userContext.DeleteWarehouseManager(userId);
            return Ok(new { message = "Warehouse manager deleted successfully" });
        }

        // =========================
        // Helpers
        // =========================

        private bool IsLoggedIn()
        {
            return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserName"));
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

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}