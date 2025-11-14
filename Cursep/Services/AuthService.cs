using System;
using System.Data.SqlClient;
using System.Windows;
using Cursep.Models;
using Cursep.Services;

namespace Cursep.Services
{
    public class AuthService
    {
        private DatabaseService _dbService;

        public AuthService()
        {
            _dbService = new DatabaseService();
        }

        public User Login(string username, string password)
        {
            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT u.*, r.RoleName, r.Description 
                        FROM Users u 
                        INNER JOIN Roles r ON u.RoleID = r.RoleID 
                        WHERE u.Username = @Username AND u.Password = @Password";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserID = (int)reader["UserID"],
                                    Username = reader["Username"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    RoleID = (int)reader["RoleID"],
                                    CreatedDate = (DateTime)reader["CreatedDate"],
                                    Role = new Role
                                    {
                                        RoleID = (int)reader["RoleID"],
                                        RoleName = reader["RoleName"].ToString(),
                                        Description = reader["Description"].ToString()
                                    }
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}", "Ошибка подключения",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }
    }
}