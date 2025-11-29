using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BillingSoftware.Models;

namespace BillingSoftware.Modules
{
    public class UserManager
    {
        private DatabaseManager dbManager;

        public UserManager()
        {
            dbManager = new DatabaseManager();
        }

        public bool AuthenticateUser(string username, string password)
        {
            string sql = "SELECT COUNT(*) FROM users WHERE username = @username AND password = @password AND is_active = 1";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public bool AddUser(User user)
        {
            try
            {
                string sql = @"INSERT INTO users (username, password, role, is_active)
                              VALUES (@username, @password, @role, @isActive)";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@password", user.Password);
                    cmd.Parameters.AddWithValue("@role", user.Role);
                    cmd.Parameters.AddWithValue("@isActive", user.IsActive ? 1 : 0);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            string sql = "SELECT * FROM users ORDER BY username";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Username = reader["username"].ToString(),
                        Password = reader["password"].ToString(),
                        Role = reader["role"].ToString(),
                        IsActive = Convert.ToInt32(reader["is_active"]) == 1
                    });
                }
            }

            return users;
        }

        public int GetActiveUsersCount()
        {
            string sql = "SELECT COUNT(*) FROM users WHERE is_active = 1";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool UpdateUser(User user)
        {
            try
            {
                string sql = @"UPDATE users SET username = @username, password = @password, 
                              role = @role, is_active = @isActive WHERE id = @id";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@password", user.Password);
                    cmd.Parameters.AddWithValue("@role", user.Role);
                    cmd.Parameters.AddWithValue("@isActive", user.IsActive ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id", user.Id);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}