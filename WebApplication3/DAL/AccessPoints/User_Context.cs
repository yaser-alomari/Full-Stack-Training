using Microsoft.Extensions.Configuration;
using Models.classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL.AccessPoints
{
    public class User_Context
    {
        private readonly string _connectionString;

        public User_Context(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // =========================
        // Users العامة
        // =========================

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetAllUsers", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader["UserId"]?.ToString() ?? "",
                    Name = reader["Name"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                    RoleName = reader["RoleName"]?.ToString() ?? ""
                });
            }

            return users;
        }

        public User? GetUserById(string userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetUserById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", int.Parse(userId));

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader["UserId"]?.ToString() ?? "",
                    Name = reader["Name"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                    RoleName = reader["RoleName"]?.ToString() ?? ""
                };
            }

            return null;
        }

        public List<User> SearchUsers(string searchText)
        {
            var users = new List<User>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spSearchUsers", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Search", searchText);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader["UserId"]?.ToString() ?? "",
                    Name = reader["Name"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                    RoleName = reader["RoleName"]?.ToString() ?? ""
                });
            }

            return users;
        }

        public User? Login(string email, string passwordHash)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spLogin", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                bool isSuccess = reader["IsSuccess"] != DBNull.Value &&
                                 Convert.ToBoolean(reader["IsSuccess"]);

                if (!isSuccess)
                    return null;

                return new User
                {
                    Id = reader["UserId"]?.ToString() ?? "",
                    Name = reader["Name"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                    RoleName = reader["RoleName"]?.ToString() ?? ""
                };
            }

            return null;
        }

        public ApiResult ChangePassword(int userId, string currentPasswordHash, string newPasswordHash)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spChangePassword", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@CurrentPasswordHash", currentPasswordHash);
            cmd.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new ApiResult
                {
                    IsSuccess = reader["IsSuccess"] != DBNull.Value && Convert.ToBoolean(reader["IsSuccess"]),
                    Message = reader["Message"]?.ToString() ?? ""
                };
            }

            return new ApiResult
            {
                IsSuccess = false,
                Message = "No response from database"
            };
        }

        public UserProfile? GetUserProfile(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetUserProfile", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                bool isSuccess = reader["IsSuccess"] != DBNull.Value &&
                                 Convert.ToBoolean(reader["IsSuccess"]);

                if (!isSuccess)
                    return null;

                return new UserProfile
                {
                    IsSuccess = true,
                    Message = reader["Message"]?.ToString() ?? "",
                    UserId = reader["UserId"]?.ToString() ?? "",
                    Name = reader["UserName"]?.ToString()
                           ?? reader["Name"]?.ToString()
                           ?? "",
                    Email = reader["UserEmail"]?.ToString()
                            ?? reader["Email"]?.ToString()
                            ?? "",
                    RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                    RoleName = reader["RoleName"]?.ToString() ?? "",
                    StatusName = reader["StatusName"]?.ToString() ?? ""
                };
            }

            return null;
        }

        // =========================
        // Doctor
        // =========================

        public void AddDoctor(Doctor doctor)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spAddDoctor", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Name", doctor.Name);
            cmd.Parameters.AddWithValue("@Email", doctor.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", doctor.PasswordHash);
            cmd.Parameters.AddWithValue("@Specialty", doctor.Specialty);
            cmd.Parameters.AddWithValue("@LicenseNumber", doctor.LicenseNumber);
            cmd.Parameters.AddWithValue("@Phone", (object?)doctor.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ClinicAddress", (object?)doctor.ClinicAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@YearsOfExperience", doctor.YearsOfExperience);
            cmd.Parameters.AddWithValue("@ResumePath", (object?)doctor.ResumePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId", doctor.StatusId);

            cmd.ExecuteNonQuery();
        }

        public void UpdateDoctor(Doctor doctor)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spUpdateDoctor", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserId", int.Parse(doctor.UserId));
            cmd.Parameters.AddWithValue("@Name", doctor.Name);
            cmd.Parameters.AddWithValue("@Email", doctor.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", doctor.PasswordHash);
            cmd.Parameters.AddWithValue("@Specialty", doctor.Specialty);
            cmd.Parameters.AddWithValue("@LicenseNumber", doctor.LicenseNumber);
            cmd.Parameters.AddWithValue("@Phone", (object?)doctor.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ClinicAddress", (object?)doctor.ClinicAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@YearsOfExperience", doctor.YearsOfExperience);
            cmd.Parameters.AddWithValue("@ResumePath", (object?)doctor.ResumePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId", doctor.StatusId);

            cmd.ExecuteNonQuery();
        }

        public void DeleteDoctor(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spDeleteDoctor", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            cmd.ExecuteNonQuery();
        }

        public List<Doctor> GetAllDoctors()
        {
            var list = new List<Doctor>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetAllDoctors", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapDoctor(reader));
            }

            return list;
        }

        public Doctor? GetDoctorByUserId(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetDoctorByUserId", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapDoctor(reader);
            }

            return null;
        }

        // =========================
        // Patient
        // =========================

        public void AddPatient(Patient patient)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spAddPatient", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Name", patient.Name);
            cmd.Parameters.AddWithValue("@Email", patient.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", patient.PasswordHash);
            cmd.Parameters.AddWithValue("@DateOfBirth", (object?)patient.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", (object?)patient.Gender ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BloodType", (object?)patient.BloodType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)patient.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)patient.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmergencyContactName", (object?)patient.EmergencyContactName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmergencyContactPhone", (object?)patient.EmergencyContactPhone ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void UpdatePatient(Patient patient)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spUpdatePatient", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserId", int.Parse(patient.UserId));
            cmd.Parameters.AddWithValue("@Name", patient.Name);
            cmd.Parameters.AddWithValue("@Email", patient.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", patient.PasswordHash);
            cmd.Parameters.AddWithValue("@DateOfBirth", (object?)patient.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", (object?)patient.Gender ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BloodType", (object?)patient.BloodType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)patient.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)patient.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmergencyContactName", (object?)patient.EmergencyContactName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmergencyContactPhone", (object?)patient.EmergencyContactPhone ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void DeletePatient(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spDeletePatient", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            cmd.ExecuteNonQuery();
        }

        public List<Patient> GetAllPatients()
        {
            var list = new List<Patient>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetAllPatients", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapPatient(reader));
            }

            return list;
        }

        public Patient? GetPatientByUserId(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetPatientByUserId", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapPatient(reader);
            }

            return null;
        }

        // =========================
        // Admin
        // =========================

        public void AddAdmin(Admin admin)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spAddAdmin", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Name", admin.Name);
            cmd.Parameters.AddWithValue("@Email", admin.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", admin.PasswordHash);
            cmd.Parameters.AddWithValue("@Phone", (object?)admin.Phone ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void UpdateAdmin(Admin admin)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spUpdateAdmin", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserId", int.Parse(admin.UserId));
            cmd.Parameters.AddWithValue("@Name", admin.Name);
            cmd.Parameters.AddWithValue("@Email", admin.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", admin.PasswordHash);
            cmd.Parameters.AddWithValue("@Phone", (object?)admin.Phone ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void DeleteAdmin(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spDeleteAdmin", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            cmd.ExecuteNonQuery();
        }

        public List<Admin> GetAllAdmins()
        {
            var list = new List<Admin>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetAllAdmins", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapAdmin(reader));
            }

            return list;
        }

        public Admin? GetAdminByUserId(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetAdminByUserId", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapAdmin(reader);
            }

            return null;
        }

        // =========================
        // Finance
        // =========================

        public void AddFinance(Finance finance)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spAddFinance", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Name", finance.Name);
            cmd.Parameters.AddWithValue("@Email", finance.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", finance.PasswordHash);
            cmd.Parameters.AddWithValue("@Phone", (object?)finance.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NationalId", (object?)finance.NationalId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)finance.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Salary", finance.Salary);
            cmd.Parameters.AddWithValue("@HireDate", (object?)finance.HireDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)finance.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId", finance.StatusId);

            cmd.ExecuteNonQuery();
        }

        public void UpdateFinance(Finance finance)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spUpdateFinance", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserId", int.Parse(finance.UserId));
            cmd.Parameters.AddWithValue("@Name", finance.Name);
            cmd.Parameters.AddWithValue("@Email", finance.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", finance.PasswordHash);
            cmd.Parameters.AddWithValue("@Phone", (object?)finance.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NationalId", (object?)finance.NationalId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)finance.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Salary", finance.Salary);
            cmd.Parameters.AddWithValue("@HireDate", (object?)finance.HireDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)finance.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId", finance.StatusId);

            cmd.ExecuteNonQuery();
        }

        public void DeleteFinance(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spDeleteFinance", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            cmd.ExecuteNonQuery();
        }

        public List<Finance> GetAllFinance()
        {
            var list = new List<Finance>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetAllFinance", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapFinance(reader));
            }

            return list;
        }

        public Finance? GetFinanceByUserId(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetFinanceByUserId", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapFinance(reader);
            }

            return null;
        }

        // =========================
        // Secretary
        // =========================

        public void AddSecretary(Secretary secretary)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spAddSecretary", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Name", secretary.Name);
            cmd.Parameters.AddWithValue("@Email", secretary.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", secretary.PasswordHash);
            cmd.Parameters.AddWithValue("@Phone", (object?)secretary.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NationalId", (object?)secretary.NationalId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)secretary.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WorkShift", (object?)secretary.WorkShift ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HireDate", (object?)secretary.HireDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)secretary.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId", secretary.StatusId);

            cmd.ExecuteNonQuery();
        }

        public void UpdateSecretary(Secretary secretary)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spUpdateSecretary", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserId", int.Parse(secretary.UserId));
            cmd.Parameters.AddWithValue("@Name", secretary.Name);
            cmd.Parameters.AddWithValue("@Email", secretary.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", secretary.PasswordHash);
            cmd.Parameters.AddWithValue("@Phone", (object?)secretary.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NationalId", (object?)secretary.NationalId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)secretary.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WorkShift", (object?)secretary.WorkShift ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HireDate", (object?)secretary.HireDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)secretary.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId", secretary.StatusId);

            cmd.ExecuteNonQuery();
        }

        public void DeleteSecretary(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spDeleteSecretary", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            cmd.ExecuteNonQuery();
        }

        public List<Secretary> GetAllSecretaries()
        {
            var list = new List<Secretary>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetAllSecretaries", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapSecretary(reader));
            }

            return list;
        }

        public Secretary? GetSecretaryByUserId(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetSecretaryByUserId", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapSecretary(reader);
            }

            return null;
        }

        // =========================
        // Warehouse Manager
        // =========================

        public void AddWarehouseManager(WarehouseManager warehouseManager)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spAddWarehouseManager", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Name", warehouseManager.Name);
            cmd.Parameters.AddWithValue("@Email", warehouseManager.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", warehouseManager.PasswordHash);
            cmd.Parameters.AddWithValue("@Phone", (object?)warehouseManager.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NationalId", (object?)warehouseManager.NationalId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)warehouseManager.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WarehouseName", (object?)warehouseManager.WarehouseName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ShiftName", (object?)warehouseManager.ShiftName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HireDate", (object?)warehouseManager.HireDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)warehouseManager.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId", warehouseManager.StatusId);

            cmd.ExecuteNonQuery();
        }

        public void UpdateWarehouseManager(WarehouseManager warehouseManager)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spUpdateWarehouseManager", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserId", int.Parse(warehouseManager.UserId));
            cmd.Parameters.AddWithValue("@Name", warehouseManager.Name);
            cmd.Parameters.AddWithValue("@Email", warehouseManager.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", warehouseManager.PasswordHash);
            cmd.Parameters.AddWithValue("@Phone", (object?)warehouseManager.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NationalId", (object?)warehouseManager.NationalId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)warehouseManager.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WarehouseName", (object?)warehouseManager.WarehouseName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ShiftName", (object?)warehouseManager.ShiftName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HireDate", (object?)warehouseManager.HireDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)warehouseManager.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId", warehouseManager.StatusId);

            cmd.ExecuteNonQuery();
        }

        public void DeleteWarehouseManager(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spDeleteWarehouseManager", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            cmd.ExecuteNonQuery();
        }

        public List<WarehouseManager> GetAllWarehouseManagers()
        {
            var list = new List<WarehouseManager>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetAllWarehouseManagers", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapWarehouseManager(reader));
            }

            return list;
        }

        public WarehouseManager? GetWarehouseManagerByUserId(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("spGetWarehouseManagerByUserId", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapWarehouseManager(reader);
            }

            return null;
        }

        // =========================
        // Mapping Methods
        // =========================

        private Doctor MapDoctor(SqlDataReader reader)
        {
            return new Doctor
            {
                UserId = reader["UserId"]?.ToString() ?? "",
                Name = reader["UserName"]?.ToString() ?? "",
                Email = reader["UserEmail"]?.ToString() ?? "",
                RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                RoleName = reader["RoleName"]?.ToString() ?? "",
                DoctorId = reader["DoctorId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DoctorId"]),
                Specialty = reader["Specialty"]?.ToString() ?? "",
                LicenseNumber = reader["LicenseNumber"]?.ToString() ?? "",
                Phone = reader["Phone"]?.ToString() ?? "",
                ClinicAddress = reader["ClinicAddress"]?.ToString() ?? "",
                YearsOfExperience = reader["YearsOfExperience"] == DBNull.Value ? 0 : Convert.ToInt32(reader["YearsOfExperience"]),
                ResumePath = reader["ResumePath"]?.ToString() ?? "",
                StatusId = reader["StatusId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StatusId"]),
                StatusName = reader["StatusName"]?.ToString() ?? ""
            };
        }

        private Patient MapPatient(SqlDataReader reader)
        {
            return new Patient
            {
                UserId = reader["UserId"]?.ToString() ?? "",
                Name = reader["UserName"]?.ToString() ?? "",
                Email = reader["UserEmail"]?.ToString() ?? "",
                RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                RoleName = reader["RoleName"]?.ToString() ?? "",
                PatientId = reader["PatientId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PatientId"]),
                DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? null : Convert.ToDateTime(reader["DateOfBirth"]),
                Gender = reader["Gender"]?.ToString() ?? "",
                BloodType = reader["BloodType"]?.ToString() ?? "",
                Phone = reader["Phone"]?.ToString() ?? "",
                Address = reader["Address"]?.ToString() ?? "",
                EmergencyContactName = reader["EmergencyContactName"]?.ToString() ?? "",
                EmergencyContactPhone = reader["EmergencyContactPhone"]?.ToString() ?? ""
            };
        }

        private Admin MapAdmin(SqlDataReader reader)
        {
            return new Admin
            {
                UserId = reader["UserId"]?.ToString() ?? "",
                Name = reader["UserName"]?.ToString() ?? "",
                Email = reader["UserEmail"]?.ToString() ?? "",
                RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                RoleName = reader["RoleName"]?.ToString() ?? "",
                AdminId = reader["AdminId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["AdminId"]),
                FullName = reader["FullName"]?.ToString() ?? "",
                Phone = reader["Phone"]?.ToString() ?? ""
            };
        }

        private Finance MapFinance(SqlDataReader reader)
        {
            return new Finance
            {
                UserId = reader["UserId"]?.ToString() ?? "",
                Name = reader["UserName"]?.ToString() ?? "",
                Email = reader["UserEmail"]?.ToString() ?? "",
                RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                RoleName = reader["RoleName"]?.ToString() ?? "",
                FinanceId = reader["FinanceId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["FinanceId"]),
                FullName = reader["FullName"]?.ToString() ?? "",
                Phone = reader["Phone"]?.ToString() ?? "",
                NationalId = reader["NationalId"]?.ToString() ?? "",
                Address = reader["Address"]?.ToString() ?? "",
                Salary = reader["Salary"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Salary"]),
                HireDate = reader["HireDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["HireDate"]),
                Notes = reader["Notes"]?.ToString() ?? "",
                StatusId = reader["StatusId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StatusId"]),
                StatusName = reader["StatusName"]?.ToString() ?? ""
            };
        }

        private Secretary MapSecretary(SqlDataReader reader)
        {
            return new Secretary
            {
                UserId = reader["UserId"]?.ToString() ?? "",
                Name = reader["UserName"]?.ToString() ?? "",
                Email = reader["UserEmail"]?.ToString() ?? "",
                RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                RoleName = reader["RoleName"]?.ToString() ?? "",
                SecretaryId = reader["SecretaryId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SecretaryId"]),
                FullName = reader["FullName"]?.ToString() ?? "",
                Phone = reader["Phone"]?.ToString() ?? "",
                NationalId = reader["NationalId"]?.ToString() ?? "",
                Address = reader["Address"]?.ToString() ?? "",
                WorkShift = reader["WorkShift"]?.ToString() ?? "",
                HireDate = reader["HireDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["HireDate"]),
                Notes = reader["Notes"]?.ToString() ?? "",
                StatusId = reader["StatusId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StatusId"]),
                StatusName = reader["StatusName"]?.ToString() ?? ""
            };
        }

        private WarehouseManager MapWarehouseManager(SqlDataReader reader)
        {
            return new WarehouseManager
            {
                UserId = reader["UserId"]?.ToString() ?? "",
                Name = reader["UserName"]?.ToString() ?? "",
                Email = reader["UserEmail"]?.ToString() ?? "",
                RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                RoleName = reader["RoleName"]?.ToString() ?? "",
                WarehouseManagerId = reader["WarehouseManagerId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["WarehouseManagerId"]),
                FullName = reader["FullName"]?.ToString() ?? "",
                Phone = reader["Phone"]?.ToString() ?? "",
                NationalId = reader["NationalId"]?.ToString() ?? "",
                Address = reader["Address"]?.ToString() ?? "",
                WarehouseName = reader["WarehouseName"]?.ToString() ?? "",
                ShiftName = reader["ShiftName"]?.ToString() ?? "",
                HireDate = reader["HireDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["HireDate"]),
                Notes = reader["Notes"]?.ToString() ?? "",
                StatusId = reader["StatusId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StatusId"]),
                StatusName = reader["StatusName"]?.ToString() ?? ""
            };
        }
    }
}
