using System;
using System.Data.SqlClient;
using System.Windows;

namespace Cursep.Services
{
    public class DatabaseService
    {
        private string connectionString = @"Data Source=stud-mssql.sttec.yar.ru,38325;Initial Catalog=user145_db;User ID=user145_db;Password=user145";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    // Проверяем, что таблицы существуют
                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'";
                    using (var command = new SqlCommand(checkTable, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (Convert.ToInt32(result) == 0)
                        {
                            MessageBox.Show("Таблицы не найдены в базе данных. Убедитесь, что скрипт создания базы выполнен.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }

                    MessageBox.Show("Подключение к базе данных успешно установлено!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
            }
            catch (SqlException ex)
            {
                string errorMessage = $"Ошибка подключения к базе данных:\n";

                switch (ex.Number)
                {
                    case -1:
                        errorMessage += "Не удается подключиться к серверу. Проверьте адрес сервера и порт.";
                        break;
                    case 2:
                        errorMessage += "Сервер не найден или недоступен.";
                        break;
                    case 4060:
                        errorMessage += "База данных не найдена.";
                        break;
                    case 18456:
                        errorMessage += "Неверный логин или пароль.";
                        break;
                    default:
                        errorMessage += ex.Message;
                        break;
                }

                MessageBox.Show(errorMessage, "Ошибка подключения",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Общая ошибка:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}