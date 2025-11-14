using System.Windows;
using System.Data.SqlClient;
using System.Collections.Generic;
using Cursep.Models;
using Cursep.Services;
using System.Diagnostics;

namespace Cursep.Views
{
    public partial class RegisterWindow : Window
    {
        private DatabaseService _dbService;
        private List<Role> _roles;

        public RegisterWindow()
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
                    string query = "SELECT * FROM Roles WHERE RoleName != 'Администратор' ORDER BY RoleName";

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

                Debug.WriteLine($"Загружено ролей: {_roles.Count}");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Ошибка загрузки ролей: {ex.Message}");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Нажата кнопка регистрации");

            if (!ValidateInput())
            {
                Debug.WriteLine("Валидация не пройдена");
                return;
            }

            Debug.WriteLine("Валидация пройдена успешно");

            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    Debug.WriteLine("Подключение к БД установлено");

                    // Проверяем, не существует ли уже пользователь с таким логином
                    string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (var checkCommand = new SqlCommand(checkUserQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                        int userCount = (int)checkCommand.ExecuteScalar();
                        Debug.WriteLine($"Проверка логина: найдено пользователей - {userCount}");

                        if (userCount > 0)
                        {
                            ShowError("Пользователь с таким логином уже существует");
                            return;
                        }
                    }

                    // Регистрируем нового пользователя
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

                        Debug.WriteLine($"Параметры: Username={txtUsername.Text}, RoleID={((Role)cmbRoles.SelectedItem).RoleID}");

                        int rowsAffected = command.ExecuteNonQuery();
                        Debug.WriteLine($"Запрос выполнен. Затронуто строк: {rowsAffected}");

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            ShowSuccess();
                            ClearForm();
                        }
                        else
                        {
                            ShowError("Не удалось зарегистрировать пользователя");
                            Debug.WriteLine("Запрос INSERT не затронул ни одной строки");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SQL ошибка: {ex.Message}, Number: {ex.Number}");

                if (ex.Number == 2627) // Ошибка уникальности
                {
                    ShowError("Пользователь с таким логином уже существует");
                }
                else if (ex.Number == 547) // Ошибка внешнего ключа
                {
                    ShowError("Ошибка: указана несуществующая роль");
                }
                else
                {
                    ShowError($"Ошибка базы данных: {ex.Message}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Общая ошибка: {ex.Message}");
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            // Сбрасываем сообщения
            HideMessages();

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("Введите логин");
                return false;
            }

            if (txtUsername.Text.Length < 3)
            {
                ShowError("Логин должен содержать не менее 3 символов");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                ShowError("Введите пароль");
                return false;
            }

            if (txtPassword.Password.Length < 4)
            {
                ShowError("Пароль должен содержать не менее 4 символов");
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

            return true;
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
            txtSuccess.Visibility = Visibility.Collapsed;
        }

        private void ShowSuccess()
        {
            txtSuccess.Visibility = Visibility.Visible;
            txtError.Visibility = Visibility.Collapsed;
        }

        private void HideMessages()
        {
            txtError.Visibility = Visibility.Collapsed;
            txtSuccess.Visibility = Visibility.Collapsed;
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            txtFullName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            if (cmbRoles.Items.Count > 0)
                cmbRoles.SelectedIndex = 0;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}