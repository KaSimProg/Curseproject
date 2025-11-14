using System.Windows;
using System.Data.SqlClient;
using System.Collections.Generic;
using Cursep.Models;
using Cursep.Services;

namespace Cursep.Views
{
    public partial class EditUserWindow : Window
    {
        private DatabaseService _dbService;
        private List<Role> _roles;
        private UserDisplay _userToEdit;

        public EditUserWindow(UserDisplay user)
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _roles = new List<Role>();
            _userToEdit = user;

            LoadUserData();
            LoadRoles();
        }

        private void LoadUserData()
        {
            txtUsername.Text = _userToEdit.Username;
            txtFullName.Text = _userToEdit.FullName;
            txtEmail.Text = _userToEdit.Email;
            txtPhone.Text = _userToEdit.Phone;
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

                // Устанавливаем текущую роль пользователя
                foreach (Role role in _roles)
                {
                    if (role.RoleName == _userToEdit.RoleName)
                    {
                        cmbRoles.SelectedItem = role;
                        break;
                    }
                }
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

                    string query = @"UPDATE Users SET 
                                    Password = @Password,
                                    FullName = @FullName,
                                    Email = @Email,
                                    Phone = @Phone,
                                    RoleID = @RoleID
                                    WHERE UserID = @UserID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Password",
                            string.IsNullOrEmpty(txtPassword.Password) ? _userToEdit.Password : txtPassword.Password);
                        command.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                        command.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        command.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        command.Parameters.AddWithValue("@RoleID", ((Role)cmbRoles.SelectedItem).RoleID);
                        command.Parameters.AddWithValue("@UserID", _userToEdit.UserID);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            this.DialogResult = true;
                            this.Close();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка обновления данных: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowError("Введите ФИО");
                return false;
            }

            if (!string.IsNullOrEmpty(txtPassword.Password) &&
                txtPassword.Password != txtConfirmPassword.Password)
            {
                ShowError("Пароли не совпадают");
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