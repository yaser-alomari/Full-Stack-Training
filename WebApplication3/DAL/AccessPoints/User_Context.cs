using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Models.classes;


namespace DAL.AccessPoints
{
    public class User_Context
    {
        private readonly string _connectionString;

        public User_Context(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("spUsers_GetAll", conn)) // اسم الـ SP
                {
                    cmd.CommandType = CommandType.StoredProcedure; // مهم جداً

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = reader["UserId"].ToString(),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Password = reader["PasswordHash"].ToString(),

                            });
                        }
                    }
                }
            }

            return users;
        }
        public void InsertUser(User user)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("spUsers_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.Password);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void UpdateUser(User user)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("spUsers_Update", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UserId", int.Parse(user.Id));
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void DeleteUser(string userId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spUsers_Delete", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    int id;
                    if (!int.TryParse(userId, out id))
                    {
                        throw new Exception("Invalid UserId");
                    }
                    cmd.Parameters.AddWithValue("@UserId", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}