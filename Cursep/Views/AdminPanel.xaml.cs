using System.Windows.Controls;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using Cursep.Models;
using Cursep.Services;
using System.Windows;

namespace Cursep.Views
{
    public partial class AdminPanel : UserControl
    {
        private User _currentUser;
        private DatabaseService _dbService;
        private List<UserDisplay> _usersList;

        public AdminPanel(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _dbService = new DatabaseService();
            _usersList = new List<UserDisplay>();

            LoadUsers();
            LoadSystemStats();
            LoadSystemLogs();
        }

        private void LoadUsers()
        {
            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT u.*, r.RoleName 
                        FROM Users u 
                        INNER JOIN Roles r ON u.RoleID = r.RoleID 
                        ORDER BY u.Username";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        _usersList.Clear();
                        while (reader.Read())
                        {
                            _usersList.Add(new UserDisplay
                            {
                                UserID = (int)reader["UserID"],
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                RoleName = reader["RoleName"].ToString(),
                                CreatedDate = (DateTime)reader["CreatedDate"],
                                IsActive = true
                            });
                        }
                        dgUsers.ItemsSource = null; // Сбрасываем источник данных
                        dgUsers.ItemsSource = _usersList; // Устанавливаем заново
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private void LoadSystemLogs()
        {
            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT TOP 50 o.*, u.Username as UserName, m.MaterialName,
                               CASE WHEN o.OperationType = 'INCOME' THEN 'Приход' ELSE 'Расход' END as OperationTypeText
                        FROM Operations o
                        INNER JOIN Users u ON o.UserID = u.UserID
                        INNER JOIN Materials m ON o.MaterialID = m.MaterialID
                        ORDER BY o.OperationDate DESC";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var logs = new List<OperationDisplay>();
                        while (reader.Read())
                        {
                            logs.Add(new OperationDisplay
                            {
                                OperationDate = (DateTime)reader["OperationDate"],
                                UserName = reader["UserName"].ToString(),
                                OperationType = reader["OperationTypeText"].ToString(),
                                MaterialName = reader["MaterialName"].ToString(),
                                Quantity = (int)reader["Quantity"],
                                Notes = reader["Notes"].ToString()
                            });
                        }
                        dgSystemLogs.ItemsSource = null;
                        dgSystemLogs.ItemsSource = logs;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки логов: {ex.Message}");
            }
        }

        private void LoadSystemStats()
        {
            try
            {
                using (var connection = _dbService.GetConnection())
                {
                    connection.Open();

                    // Всего пользователей
                    string usersCount = "SELECT COUNT(*) FROM Users";
                    using (var cmd = new SqlCommand(usersCount, connection))
                    {
                        txtTotalUsers.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Операций сегодня
                    string todayOps = "SELECT COUNT(*) FROM Operations WHERE CAST(OperationDate AS DATE) = CAST(GETDATE() AS DATE)";
                    using (var cmd = new SqlCommand(todayOps, connection))
                    {
                        txtTodayOperations.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Активные сессии
                    txtActiveSessions.Text = "1";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        private void BtnRefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            RefreshAllData();
        }

        // НОВЫЙ МЕТОД: Обновление всех данных
        private void RefreshAllData()
        {
            LoadUsers();
            LoadSystemStats();
            LoadSystemLogs();
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow();
            if (addUserWindow.ShowDialog() == true)
            {
                RefreshAllData();
                MessageBox.Show("Пользователь успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования");
                return;
            }

            var selectedUser = dgUsers.SelectedItem as UserDisplay;

            // Открываем окно редактирования
            var editUserWindow = new EditUserWindow(selectedUser);
            if (editUserWindow.ShowDialog() == true)
            {
                RefreshAllData();
                MessageBox.Show("Данные пользователя обновлены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnBlockUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для удаления");
                return;
            }

            var selectedUser = dgUsers.SelectedItem as UserDisplay;

            // Нельзя удалить самого себя
            if (selectedUser.UserID == _currentUser.UserID)
            {
                MessageBox.Show("Нельзя удалить собственный аккаунт!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{selectedUser.Username}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var connection = _dbService.GetConnection())
                    {
                        connection.Open();

                        // Сначала удаляем связанные операции
                        string deleteOperations = "DELETE FROM Operations WHERE UserID = @UserID";
                        using (var cmd1 = new SqlCommand(deleteOperations, connection))
                        {
                            cmd1.Parameters.AddWithValue("@UserID", selectedUser.UserID);
                            cmd1.ExecuteNonQuery();
                        }

                        // Затем удаляем пользователя
                        string deleteUser = "DELETE FROM Users WHERE UserID = @UserID";
                        using (var cmd2 = new SqlCommand(deleteUser, connection))
                        {
                            cmd2.Parameters.AddWithValue("@UserID", selectedUser.UserID);
                            int rowsAffected = cmd2.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                RefreshAllData();
                                MessageBox.Show("Пользователь успешно удален!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Настройки сохранены успешно!");
        }

        // НОВЫЙ МЕТОД: Обновление при активации вкладки
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                RefreshAllData();
            }
        }
    }
}