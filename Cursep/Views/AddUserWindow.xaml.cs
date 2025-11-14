using System.Windows;
using System.Data.SqlClient;
using System.Collections.Generic;
using Cursep.Models;
using Cursep.Services;

namespace Cursep.Views
{
    public partial class AddUserWindow : Window
    {
        private DatabaseService _dbService;
        private List<Role> _roles;

        public AddUserWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _roles = new List<Role>();

            LoadRoles();
        }

        private void LoadRoles()
        {
            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM Roles ORDER BY RoleName";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _roles.Add(new Role
                            {
                                RoleID = (int)reader["RoleID"],
                                RoleName = reader["RoleName"].ToString(),
                                Description = reader["Description"].ToString()
                            });
                        }
                    }
                }

                cmbRoles.ItemsSource = _roles;
                if (_roles.Count > 0)
                    cmbRoles.SelectedIndex = 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO Users (Username, Password, FullName, Email, Phone, RoleID)
                        VALUES (@Username, @Password, @FullName, @Email, @Phone, @RoleID)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                        command.Parameters.AddWithValue("@Password", txtPassword.Password);
                        command.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                        command.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        command.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        command.Parameters.AddWithValue("@RoleID", ((Role)cmbRoles.SelectedItem).RoleID);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Ошибка уникальности
                {
                    ShowError("Пользователь с таким логином уже существует");
                }
                else
                {
                    ShowError($"Ошибка базы данных: {ex.Message}");
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("Введите логин");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                ShowError("Введите пароль");
                return false;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                ShowError("Пароли не совпадают");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowError("Введите ФИО");
                return false;
            }

            if (cmbRoles.SelectedItem == null)
            {
                ShowError("Выберите роль");
                return false;
            }

            HideError();
            return true;
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            txtError.Visibility = Visibility.Collapsed;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}